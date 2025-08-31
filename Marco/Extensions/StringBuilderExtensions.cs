using Cysharp.Text;

namespace Marco.Extensions;

internal static class StringBuilderExtensions
{
    /// <summary>
    ///     Determines whether the specified <see cref="Utf16ValueStringBuilder" /> contains the specified span of characters.
    /// </summary>
    /// <param name="builder">The builder to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="value" /> is found within <paramref name="builder" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool Contains(this Utf16ValueStringBuilder builder, ReadOnlySpan<char> value)
    {
        if (builder.Length < value.Length)
        {
            return false;
        }

        ReadOnlySpan<char> span = builder.AsSpan();
        int length = Math.Min(builder.Length, value.Length);

        for (var index = 0; index <= builder.Length - value.Length; index++)
        {
            bool found = true;
            for (var innerIndex = 0; innerIndex < length; innerIndex++)
            {
                if (span[index + innerIndex] != value[innerIndex])
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Determines whether the specified <see cref="Utf16ValueStringBuilder" /> starts with the specified span of characters.
    /// </summary>
    /// <param name="builder">The builder to search.</param>
    /// <param name="value">The value to search for.</param>
    /// <returns>
    ///     <see langword="true" /> if <paramref name="value" /> is found at the start of <paramref name="builder" />; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    public static bool StartsWith(this Utf16ValueStringBuilder builder, ReadOnlySpan<char> value)
    {
        if (builder.Length < value.Length)
        {
            return false;
        }

        ReadOnlySpan<char> span = builder.AsSpan();
        int length = Math.Min(builder.Length, value.Length);

        for (var index = 0; index < length; index++)
        {
            if (span[index] != value[index])
            {
                return false;
            }
        }

        return true;
    }
}
