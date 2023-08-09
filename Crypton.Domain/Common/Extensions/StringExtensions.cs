// <copyright file="StringExtensions.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Crypton.Domain.Common.Extensions;

/// <summary>
/// String extensions.
/// </summary>
public static partial class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var startUnderscores = Word().Match(input);
        return startUnderscores + CamelCase().Replace(input, "$1_$2").ToLower();
    }

    public static string CalculateSha256HexDigest(this string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        var digest = BitConverter.ToString(hash);
        return digest.Replace("-", string.Empty).ToLower();
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