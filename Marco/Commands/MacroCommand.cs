using Cysharp.Text;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Extensions;
using Marco.Services;

namespace Marco.Commands;

internal sealed class MacroCommand : ApplicationCommandModule
{
    private readonly MacroService _macroService;
    private readonly MacroCooldownService _cooldownService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    /// <param name="cooldownService">The cooldown service.</param>
    public MacroCommand(MacroService macroService, MacroCooldownService cooldownService)
    {
        _macroService = macroService;
        _cooldownService = cooldownService;
    }

    [SlashCommand("anonmacro", "Executes an macro anonymously, hiding the invocation.", false)]
    [SlashRequireGuild]
    [UsedImplicitly]
    public async Task AnonymousMacroAsync(InteractionContext context,
        [Option("macro", "The name of the macro.", true)] [Autocomplete(typeof(MacroAutocompleteProvider))] string macroName,
        [Option("mention", "The user to mention.")] DiscordUser? mentionUser = null)
    {
        Macro? macro = await VerifyMacroAsync(context, macroName);
        if (macro is null)
        {
            return;
        }

        var builder = new DiscordMessageBuilder();
        using Utf16ValueStringBuilder response = ZString.CreateStringBuilder();
        response.Append(macro.Response);
        bool isSilent = response.StartsWith("@silent ");

        if (isSilent)
        {
            response.Remove(0, 8);
        }

        if (mentionUser is not null)
        {
            if (!response.Contains($"<@{mentionUser.Id}>") && !response.Contains($"<@!{mentionUser.Id}>"))
            {
                response.AppendLine();
                response.AppendLine();
                response.Append(mentionUser.Mention);
            }

            builder.AddMention(new UserMention(mentionUser));
        }
        else if (isSilent)
        {
            builder.SuppressNotifications();
        }

        builder.WithContent(response.ToString());
        await context.CreateResponseAsync("The macro has been sent.", true);
        await context.Channel.SendMessageAsync(builder);
    }

    [SlashCommand("macro", "Executes a macro.")]
    [SlashRequireGuild]
    [UsedImplicitly]
    public async Task MacroAsync(InteractionContext context,
        [Option("macro", "The name of the macro.", true)] [Autocomplete(typeof(MacroAutocompleteProvider))] string macroName,
        [Option("mention", "The user to mention.")] DiscordUser? mentionUser = null)
    {
        Macro? macro = await VerifyMacroAsync(context, macroName);
        if (macro is null)
        {
            return;
        }

        var builder = new DiscordInteractionResponseBuilder();
        using Utf16ValueStringBuilder response = ZString.CreateStringBuilder();
        response.Append(macro.Response);
        bool isSilent = response.StartsWith("@silent ");

        if (isSilent)
        {
            response.Remove(0, 8);
        }

        if (mentionUser is not null)
        {
            if (!response.Contains($"<@{mentionUser.Id}>") && !response.Contains($"<@!{mentionUser.Id}>"))
            {
                response.AppendLine();
                response.AppendLine();
                response.Append(mentionUser.Mention);
            }

            builder.AddMention(new UserMention(mentionUser));
        }
        else if (isSilent)
        {
            builder.SuppressNotifications();
        }

        builder.WithContent(response.ToString());
        await context.CreateResponseAsync(builder);
        _cooldownService.UpdateCooldown(context.Channel, macro);
    }

    private async Task<Macro?> VerifyMacroAsync(InteractionContext context, string macroName)
    {
        if (!_macroService.TryGetMacro(context.Guild, macroName, out Macro? macro))
        {
            await context.CreateResponseAsync($"The macro `{macroName}` doesn't exist.", true);
            return null;
        }

        DiscordChannel channel = context.Channel;

        if (macro.ChannelId.HasValue && macro.ChannelId.Value != channel.Id)
        {
            await context.CreateResponseAsync($"The macro `{macroName}` cannot be executed here.", true);
            return null;
        }

        if (_cooldownService.IsOnCooldown(channel, macro))
        {
            await context.CreateResponseAsync($"The macro `{macroName}` is on cooldown because it was very recently executed.", true);
            return null;
        }

        return macro;
    }
}
