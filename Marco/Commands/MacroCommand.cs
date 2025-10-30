using System.ComponentModel;
using Cysharp.Text;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Extensions;
using Marco.Services;

namespace Marco.Commands;

internal sealed class MacroCommand
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

    [Command("anonmacro")]
    [Description("Executes an macro anonymously, hiding the invocation.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task AnonymousMacroAsync(SlashCommandContext context,
        [Parameter("name"), Description("The name of the macro."), SlashAutoCompleteProvider<MacroAutocompleteProvider>]
        string name,
        [Parameter("mention"), Description("The user to mention.")] DiscordUser? mentionUser = null)
    {
        Macro? macro = await VerifyMacroAsync(context, name);
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
        await context.RespondAsync("The macro has been sent.", true);
        await context.Channel.SendMessageAsync(builder);
    }

    [Command("macro")]
    [Description("Executes a macro.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task MacroAsync(SlashCommandContext context,
        [Parameter("name"), Description("The name of the macro."), SlashAutoCompleteProvider<MacroAutocompleteProvider>]
        string name,
        [Parameter("mention"), Description("The user to mention.")] DiscordUser? mentionUser = null)
    {
        Macro? macro = await VerifyMacroAsync(context, name);
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
        await context.RespondAsync(builder);
        _cooldownService.UpdateCooldown(context.Channel, macro);
    }

    private async Task<Macro?> VerifyMacroAsync(SlashCommandContext context, string macroName)
    {
        if (context.Guild is not { } guild || !_macroService.TryGetMacro(guild, macroName, out Macro? macro))
        {
            await context.RespondAsync($"The macro `{macroName}` doesn't exist.", true);
            return null;
        }

        DiscordChannel channel = context.Channel;

        if (macro.ChannelId.HasValue && macro.ChannelId.Value != channel.Id)
        {
            await context.RespondAsync($"The macro `{macroName}` cannot be executed here.", true);
            return null;
        }

        if (_cooldownService.IsOnCooldown(channel, macro))
        {
            await context.RespondAsync($"The macro `{macroName}` is on cooldown because it was very recently executed.", true);
            return null;
        }

        return macro;
    }
}
