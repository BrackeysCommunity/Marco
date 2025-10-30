using System.ComponentModel;
using System.Text;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Entities;
using Humanizer;
using JetBrains.Annotations;
using Marco.Extensions;
using Marco.Services;

namespace Marco.Commands;

/// <summary>
///     Represents a class which implements the <c>info</c> command.
/// </summary>
internal sealed class InfoCommand
{
    private readonly BotService _botService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="InfoCommand" /> class.
    /// </summary>
    /// <param name="botService">The bot service.</param>
    public InfoCommand(BotService botService)
    {
        _botService = botService;
    }

    [Command("info")]
    [Description("Displays information about the bot.")]
    [RequireGuild]
    [UsedImplicitly]
    public async Task InfoAsync(SlashCommandContext context)
    {
        DiscordGuild guild = context.Guild!;
        DiscordClient client = context.Client;
        DiscordMember member = (await client.CurrentUser.GetAsMemberOfAsync(guild))!;
        string botVersion = _botService.Version;
        DiscordColor embedColor = member.Color.PrimaryColor;
        var ping = (int)client.GetConnectionLatency(guild.Id).TotalMilliseconds;

        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(member);
        embed.WithColor(embedColor);
        embed.WithThumbnail(member.AvatarUrl);
        embed.WithTitle($"Marco v{botVersion}");
        embed.AddField("Ping", $"{ping} ms", true);
        embed.AddField("Uptime", (DateTimeOffset.UtcNow - _botService.StartedAt).Humanize(), true);
        embed.AddField("Source", "[View on GitHub](https://github.com/BrackeysCommunity/Marco)", true);

        var builder = new StringBuilder();
        builder.AppendLine($"Marco: {botVersion}");
        builder.AppendLine($"D#+: {client.VersionString}");
        builder.AppendLine($"CLR: {Environment.Version.ToString(3)}");
        builder.AppendLine($"Host: {Environment.OSVersion}");

        embed.AddField("Version", Formatter.BlockCode(builder.ToString()));
        await context.RespondAsync(embed, true);
    }
}
