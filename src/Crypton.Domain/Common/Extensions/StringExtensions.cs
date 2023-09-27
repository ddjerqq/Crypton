using System.Text.RegularExpressions;

namespace Crypton.Domain.Common.Extensions;

/// <summary>
/// String extensions.
/// </summary>
public static class StringExtensions
{
    private const string WordPattern = @"^_+";
    private const string CamelCasePattern = @"([a-z0-9])([A-Z])";

    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var startUnderscores = Regex.Match(input, WordPattern);
        return startUnderscores + Regex.Replace(input, CamelCasePattern, "$1_$2").ToLower();
    }
}