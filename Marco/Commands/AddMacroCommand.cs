using System.ComponentModel;
using System.Text.RegularExpressions;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Marco.Services;

namespace Marco.Commands;

internal sealed class AddMacroCommand
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public AddMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [Command("addmacro")]
    [Description("Adds a new macro.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task AddMacro(
        SlashCommandContext context,
        [Parameter("name"), Description("The name of the macro.")] string name
    )
    {
        var embed = new DiscordEmbedBuilder();

        DiscordGuild guild = context.Guild!;

        if (string.IsNullOrWhiteSpace(name))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription("No name was specified.");
            await context.RespondAsync(embed, true);
            return;
        }

        name = Regex.Replace(name.ToLowerInvariant(), "\\s", string.Empty, RegexOptions.Compiled);

        if (_macroService.TryGetMacro(guild, name, out _))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot add macro");
            embed.WithDescription($"A macro with the name or alias `{name}` already exists.");
            await context.RespondAsync(embed, true);
            return;
        }

        var modal = new DiscordModalBuilder();
        modal.WithCustomId($"add-macro_{name}");
        modal.WithTitle($"Add macro '{name}'");

        var aliasInputComponent = new DiscordTextInputComponent(
            customId: "aliases",
            placeholder: "e.g. null nullreference nullref",
            required: false,
            style: DiscordTextInputStyle.Short);

        var responseInputComponent = new DiscordTextInputComponent(
            customId: "response",
            required: true,
            style: DiscordTextInputStyle.Paragraph);

        modal.AddTextInput(aliasInputComponent, "Aliases (space-separated)", "Alternative names for the macro");
        modal.AddTextInput(responseInputComponent, "Response");

        await context.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
    }
}
