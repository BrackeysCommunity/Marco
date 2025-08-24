using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Marco.Extensions;

/// <summary>
///     Extension methods for <see cref="DiscordGuild" />.
/// </summary>
internal static class DiscordGuildExtensions
{
    /// <summary>
    ///     Gets a guild member by their ID. If the member is not found, <see langword="null" /> is returned instead of
    ///     <see cref="NotFoundException" /> being thrown.
    /// </summary>
    /// <param name="guild">The guild whose member list to search.</param>
    /// <param name="userId">The ID of the member to retrieve.</param>
    /// <exception cref="ArgumentNullException"><paramref name="guild" /> is <see langword="null" />.</exception>
    public static async Task<DiscordMember?> GetMemberOrNullAsync(this DiscordGuild guild, ulong userId)
    {
        if (guild is null)
        {
            throw new ArgumentNullException(nameof(guild));
        }

        try
        {
            // we should never use exceptions for flow control but this is D#+ we're talking about.
            // NotFoundException isn't even documented, and yet it gets thrown when a member doesn't exist.
            // so this method should hopefully clearly express that - and at least using exceptions for flow control *here*,
            // removes the need to do the same in consumer code.
            // god I hate this.
            return await guild.GetMemberAsync(userId);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }
}
