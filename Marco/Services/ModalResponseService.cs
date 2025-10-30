using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Marco.Data;
using Marco.Extensions;
using Microsoft.Extensions.Logging;

namespace Marco.Services;

/// <summary>
///     Represents a service which handles modal responses.
/// </summary>
internal sealed class ModalResponseService : IEventHandler<ModalSubmittedEventArgs>
{
    private const string AddMacroPrefix = "add-macro_";
    private const string EditMacroPrefix = "edit-macro_";
    private readonly Dictionary<string, Func<ModalSubmittedEventArgs, string, Task>> _modalHandlers = [];
    private readonly ILogger<ModalResponseService> _logger;
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModalResponseService" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="macroService">The macro service.</param>
    public ModalResponseService(ILogger<ModalResponseService> logger, MacroService macroService)
    {
        _logger = logger;
        _macroService = macroService;
        _modalHandlers[AddMacroPrefix] = AddMacroAsync;
        _modalHandlers[EditMacroPrefix] = EditMacroAsync;
    }

    /// <inheritdoc />
    public async Task HandleEventAsync(DiscordClient sender, ModalSubmittedEventArgs e)
    {
        foreach ((string prefix, Func<ModalSubmittedEventArgs, string, Task> handler) in _modalHandlers)
        {
            if (!e.Id.StartsWith(prefix))
            {
                continue;
            }

            await handler(e, e.Id[prefix.Length..]);
            return;
        }
    }

    private async Task AddMacroAsync(ModalSubmittedEventArgs e, string name)
    {
        var response = new DiscordInteractionResponseBuilder();
        response.AsEphemeral();

        var embed = new DiscordEmbedBuilder();
        DiscordGuild? guild = e.Interaction.Guild;

        if (guild is null)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription("This command can only be used in a guild.");
            response.AddEmbed(embed);

            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
            return;
        }

        var aliasInput = e.Values["aliases"] as TextInputModalSubmission;
        var responseInput = e.Values["response"] as TextInputModalSubmission;

        string? value = responseInput?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("No response was specified.");
        }

        var aliases = new List<string>(aliasInput?.Value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []);
        var invalidAliases = new List<string>();

        if (aliases.Count > 0)
        {
            for (int index = aliases.Count - 1; index >= 0; index--)
            {
                string current = aliases[index];
                if (_macroService.TryGetMacro(guild, current, out _))
                {
                    aliases.RemoveAt(index);
                    invalidAliases.Add(current);
                }
            }
        }

        Macro macro = await _macroService.CreateMacroAsync(guild, null, name, value, aliases.ToArray());
        embed.WithColor(DiscordColor.Green);
        embed.WithTitle("Macro added");
        embed.WithDescription($"The macro `{macro.Name}` has been added.");
        if (invalidAliases.Count > 0)
        {
            embed.Description += "\n\n⚠️ The following aliases were not added because they already exist: " +
                                 string.Join(", ", invalidAliases.Select(a => $"`{a}`"));
        }

        embed.AddField("Type", "Global", true); // TODO: support channel-specific macros
        embed.AddFieldIf(aliases.Count > 0, () => "Alias".ToQuantity(aliases.Count), () => string.Join(' ', aliases), true);

        value = macro.Response;
        if (value is { Length: > 1024 })
        {
            value = $"{value[..1021]}...";
        }

        embed.AddField("Response", value);

        response.AddEmbed(embed);
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }

    private async Task EditMacroAsync(ModalSubmittedEventArgs e, string name)
    {
        var response = new DiscordInteractionResponseBuilder();
        response.AsEphemeral();

        var embed = new DiscordEmbedBuilder();
        DiscordGuild? guild = e.Interaction.Guild;

        if (guild is null)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription("This command can only be used in a guild.");
            response.AddEmbed(embed);

            await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
            return;
        }

        try
        {
            var aliasInput = e.Values["aliases"] as TextInputModalSubmission;
            var responseInput = e.Values["response"] as TextInputModalSubmission;

            string? value = responseInput?.Value;
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException("No response was specified.");
            }

            var aliases = new List<string>(aliasInput?.Value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? []);

            Macro macro = await _macroService.EditMacroAsync(guild, name, m =>
            {
                m.Aliases = aliases;
                m.Response = value;
            });

            _logger.LogInformation("Macro '{Name}' edited in {Guild}. {Count} aliases: {Aliases}, Response: {Response}",
                name,
                guild,
                aliases.Count,
                string.Join(' ', aliases),
                response);

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro edited");
            embed.WithDescription($"The macro `{macro.Name}` has been edited.");
            embed.AddField("Name", macro.Name, true);
            embed.AddField("Type", "Global", true); // TODO: support channel-specific macros
            embed.AddFieldIf(aliases.Count > 0, () => "Alias".ToQuantity(aliases.Count), () => string.Join(' ', aliases), true);

            if (value is { Length: > 1024 })
            {
                value = $"{value[..1021]}...";
            }

            embed.AddField("Response", value);
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().Name);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription(exception.Message);

            _logger.LogError(exception, "Could not edit macro '{MacroName}' in {Guild}", name, guild);
        }

        response.AddEmbed(embed);
        await e.Interaction.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
    }
}
