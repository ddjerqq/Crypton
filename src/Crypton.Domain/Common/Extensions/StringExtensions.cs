using System.Text.RegularExpressions;

namespace Crypton.Domain.Common.Extensions;

/// <summary>
/// String extensions.
/// </summary>
public static partial class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var startUnderscores = Word().Match(input);
        return startUnderscores + CamelCase().Replace(input, "$1_$2").ToLower();
    }

    /// <summary>
    /// regex for words
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("^_+")]
    private static partial Regex Word();

    /// <summary>
    /// camel case word
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("([a-z0-9])([A-Z])")]
    private static partial Regex CamelCase();
}