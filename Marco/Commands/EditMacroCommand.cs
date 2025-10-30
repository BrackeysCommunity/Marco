using System.ComponentModel;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Marco.AutocompleteProviders;
using Marco.Data;
using Marco.Services;
using DiscordModalBuilder = DSharpPlus.Entities.DiscordModalBuilder;

namespace Marco.Commands;

internal sealed class EditMacroCommand
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditMacroCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public EditMacroCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [Command("editmacro")]
    [Description("Edits an existing macro.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task EditMacroAsync(
        SlashCommandContext context,
        [Parameter("name"), Description("The name of the macro."),
         SlashAutoCompleteProvider<MacroAutocompleteProvider>]
        string name
    )
    {
        var embed = new DiscordEmbedBuilder();

        if (string.IsNullOrWhiteSpace(name))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription("No name was specified.");
            await context.RespondAsync(embed, true);
            return;
        }

        DiscordGuild guild = context.Guild!;
        if (!_macroService.TryGetMacro(guild, name, out Macro? macro))
        {
            embed.WithColor(DiscordColor.Red);
            embed.WithTitle("Cannot edit macro");
            embed.WithDescription($"No macro with the name `{name}` was found.");
            await context.RespondAsync(embed, true);
            return;
        }

        var modal = new DiscordModalBuilder();
        modal.WithCustomId($"edit-macro_{macro.Name}");
        modal.WithTitle($"Edit macro '{macro.Name}'");

        var aliasInputComponent = new DiscordTextInputComponent(
            customId: "aliases",
            placeholder: "e.g. null nullreference nullref",
            value: string.Join(' ', macro.Aliases),
            required: false,
            style: DiscordTextInputStyle.Short);

        var responseInputComponent = new DiscordTextInputComponent(
            customId: "response",
            value: macro.Response,
            required: true,
            style: DiscordTextInputStyle.Paragraph);

        modal.AddTextInput(aliasInputComponent, "Aliases (space-separated)", "Alternative names for the macro");
        modal.AddTextInput(responseInputComponent, "Response");

        await context.Interaction.CreateResponseAsync(DiscordInteractionResponseType.Modal, modal);
    }
}
