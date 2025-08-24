using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Marco.Extensions;

/// <summary>
///     Extension methods for <see cref="DiscordUser" />.
/// </summary>
internal static class DiscordUserExtensions
{
    /// <summary>
    ///     Returns the current <see cref="DiscordUser" /> as a member of the specified guild.
    /// </summary>
    /// <param name="user">The user to transform.</param>
    /// <param name="guild">The guild whose member list to search.</param>
    /// <returns>
    ///     A <see cref="DiscordMember" /> whose <see cref="DiscordMember.Guild" /> is equal to <paramref name="guild" />, or
    ///     <see langword="null" /> if this user is not in the specified <paramref name="guild" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="guild" /> is <see langword="null" />.</para>
    /// </exception>
    public static async Task<DiscordMember?> GetAsMemberOfAsync(this DiscordUser user, DiscordGuild guild)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (guild is null)
        {
            throw new ArgumentNullException(nameof(guild));
        }

        if (user is DiscordMember member && member.Guild == guild)
        {
            return member;
        }

        if (guild.Members.TryGetValue(user.Id, out member!))
        {
            return member;
        }

        try
        {
            return await guild.GetMemberAsync(user.Id);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    ///     Returns the user's username with the discriminator, in the format <c>username#discriminator</c>.
    /// </summary>
    /// <param name="user">The user whose username and discriminator to retrieve.</param>
    /// <returns>A string in the format <c>username#discriminator</c></returns>
    /// <exception cref="ArgumentNullException"><paramref name="user" /> is <see langword="null" />.</exception>
    public static string GetUsernameWithDiscriminator(this DiscordUser user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (user.Discriminator == "0")
        {
            // user has a new username. see: https://discord.com/blog/usernames
            return user.Username;
        }

        return $"{user.Username}#{user.Discriminator}";
    }
}
