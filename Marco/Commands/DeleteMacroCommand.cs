using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Marco.AutocompleteProviders;
using Marco.Services;

namespace Marco.Commands;

internal sealed class DeleteMacroCommand
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DeleteMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public DeleteMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [Command("deletemacro")]
    [Description("Deletes an existing macro.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task DeleteMacroAsync(
        SlashCommandContext context,
        [Parameter("name"), Description("The name of the macro.")]
        [SlashAutoCompleteProvider<MacroAutocompleteProvider>]
        string name
    )
    {
        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(name))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot delete macro");
            embed.WithDescription("No name was specified.");
            await context.RespondAsync(embed, true);
            return;
        }

        try
        {
            await _macroService.DeleteMacroAsync(context.Guild, name);

            embed.WithColor(DiscordColor.Green);
            embed.WithTitle("Macro deleted");
            embed.WithDescription($"The macro `{name}` has been deleted.");
        }
        catch (Exception exception)
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithAuthor(exception.GetType().Name);
            embed.WithTitle("Cannot delete macro");
            embed.WithDescription(exception.Message);
        }

        await context.RespondAsync(embed);
    }
}
