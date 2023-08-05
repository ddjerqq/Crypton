// <copyright file="Rules.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace Crypton.Infrastructure.Diamond;

/// <summary>
/// Salt - random Guid md5 hashed
/// HashHead - random int in range: [0x1000, 0xffff]
/// HashTail - random int in range: [0x10000000, 0x7fffffff] but in hex
/// ChecksumStart - random byte full range
/// ChecksumIndexes - 32 uints all in range [0, 64)
/// HashFormat - {HashHead}:{Sha1 of payload}:{checksum hex}:{HashTail}
/// AppToken - two random longs bit shifted, converted to hex, and combined
///
/// the payload is in the format: var payload {Salt}{timestampMs}{extractedUri}{userId}.
/// </summary>
public sealed class Rules : IRules
{
    private Rules(Guid salt, int hashHead, int hashTail, byte checksumStart, IEnumerable<int> checksumIndexes, UInt128 appToken)
    {
        this.Salt = salt;
        this.HashHead = hashHead;
        this.HashTail = hashTail;
        this.ChecksumIndexes = checksumIndexes.ToArray();
        this.ChecksumStart = checksumStart;
        this.AppToken = appToken;
    }

    public Guid Salt { get; init; }

    [JsonIgnore]
    public int HashHead { get; init; }

    [JsonIgnore]
    public int HashTail { get; init; }

    public byte ChecksumStart { get; init; }

    public int[] ChecksumIndexes { get; init; }

    [JsonIgnore]
    public UInt128 AppToken { get; init; }

    [JsonPropertyName("app_token")]
    public string AppTokenDigest => this.AppToken.ToString("x16");

    public string HashFormat => $"{this.HashHead:x4}:{{0}}:{{1}}:{this.HashTail:x8}";

    public static Rules Random()
    {
        var salt = Guid.NewGuid();

        int hashHead = RandomNumberGenerator.GetInt32(0x0, 0xffff);
        int hashTail = RandomNumberGenerator.GetInt32(0x0, 0x7fffffff);

        byte checksumStart = RandomNumberGenerator.GetBytes(1).First();

        var checksumIndexes = Enumerable
            .Range(0, 32)
            .Select(_ => RandomNumberGenerator.GetInt32(0, 40));

        var appToken = new UInt128(
            (ulong)System.Random.Shared.NextInt64(),
            (ulong)System.Random.Shared.NextInt64());

        return CreateInstance(salt, hashHead, hashTail, checksumStart, checksumIndexes, appToken);
    }

    public string? Sign(string? uri, string? userId, DateTime? timestamp)
    {
        if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(userId) || !timestamp.HasValue)
            return null;

        timestamp = timestamp.Value.ToUniversalTime();

        var parsedUri = new Uri(uri);
        var parsedUrl = $"{parsedUri.AbsolutePath}{parsedUri.Query}";

        // 2023-08-05T20:52:11.288Z
        // to make this compliant with the javascript ISO format.
        var payload = $"{this.Salt}:{timestamp:yyyy-MM-dd'T'HH:mm:ss'.'fff'Z'}:{parsedUrl}:{userId}";

        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        string digest = string.Concat(hash.Select(b => b.ToString("x2")));

        byte[] digestBytes = Encoding.UTF8.GetBytes(digest);
        var sum = (int)this.ChecksumIndexes.Aggregate(this.ChecksumStart, (sum, idx) => (byte)(sum + digestBytes[idx]));
        sum = Math.Abs(sum);
        Console.WriteLine(sum);

        return string.Format(this.HashFormat, digest, $"{sum:D3}");
    }

    internal static Rules CreateInstance(
        Guid salt,
        int hashHead,
        int hashTail,
        byte checksumStart,
        IEnumerable<int> checksumIndexes,
        UInt128 appToken)
    {
        return new Rules(salt, hashHead, hashTail, checksumStart, checksumIndexes, appToken);
    }
}