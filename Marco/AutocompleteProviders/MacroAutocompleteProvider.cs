using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using Marco.Data;
using Marco.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Marco.AutocompleteProviders;

internal sealed class MacroAutocompleteProvider : IAutoCompleteProvider
{
    /// <inheritdoc />
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        MacroService macroService = context.ServiceProvider.GetRequiredService<MacroService>();
        IReadOnlyCollection<Macro> macros = macroService.GetMacros(context.Guild!);

        var result = new List<DiscordAutoCompleteChoice>();
        string optionValue = context.UserInput ?? string.Empty;
        bool hasOptionValue = !string.IsNullOrWhiteSpace(optionValue);

        foreach (Macro macro in macros)
        {
            ulong? channelId = macro.ChannelId;
            if (channelId.HasValue && channelId.Value != context.Channel.Id)
            {
                continue;
            }

            if (hasOptionValue && !macro.Name.Contains(optionValue) && !macro.Aliases.Contains(optionValue))
            {
                continue;
            }

            result.Add(new DiscordAutoCompleteChoice(macro.Name, macro.Name));

            if (result.Count >= 25)
            {
                // Discord only allows 25 choices per autocomplete response
                break;
            }
        }

        result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
        return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(result);
    }
}
