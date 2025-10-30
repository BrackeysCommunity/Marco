using System.ComponentModel;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Humanizer;
using JetBrains.Annotations;
using Marco.Data;
using Marco.Services;

namespace Marco.Commands;

internal sealed class ListMacrosCommand
{
    private readonly MacroService _macroService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ListMacrosCommand" /> class.
    /// </summary>
    /// <param name="macroService">The macro service.</param>
    public ListMacrosCommand(MacroService macroService)
    {
        _macroService = macroService;
    }

    [Command("listmacros")]
    [Description("List all available macros.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task ListMacrosAsync(SlashCommandContext context)
    {
        IReadOnlyCollection<Macro> macros = _macroService.GetMacros(context.Guild!).Distinct().ToArray();
        var embed = new DiscordEmbedBuilder();
        embed.WithColor(DiscordColor.CornflowerBlue);
        embed.WithTitle($"{"macro".ToQuantity(macros.Count)} available");

        if (macros.Count > 0)
        {
            foreach (IGrouping<ulong?, Macro> grouping in macros.GroupBy(m => m.ChannelId))
            foreach (Macro[] group in grouping.Chunk(20))
            {
                var names = new List<string>();
                string type = grouping.Key.HasValue
                    ? (await context.Client.GetChannelAsync(grouping.Key.Value)).Name
                    : "Global";

                foreach (Macro macro in group.OrderBy(m => m.Name))
                    names.Add(macro.Name);

                embed.AddField(type, Formatter.BlockCode(string.Join('\n', names)), true);
            }
        }

        await context.RespondAsync(embed);
    }
}
