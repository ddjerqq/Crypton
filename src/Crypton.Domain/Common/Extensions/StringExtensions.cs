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

    private static int GetHexVal(char hex) => hex - (hex < 58 ? 48 : hex < 97 ? 55 : 87);

    public static byte[] ToHexBytes(this string hex)
    {
        // add a 0 before the last character
        if (hex.Length % 2 == 1)
            hex = hex.Insert(hex.Length - 1, "0");

        byte[] arr = new byte[hex.Length >> 1];

        for (int i = 0; i < hex.Length >> 1; ++i)
            arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + GetHexVal(hex[(i << 1) + 1]));

        return arr;
    }
}
