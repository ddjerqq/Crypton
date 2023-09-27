using System.Security.Cryptography;
using System.Text;

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
public sealed class Rules
{
    private const int RotateAt = 10_000;
    private static int _accessCount = 0;
    private static Rules _instance = NewRules();

    public static Rules Shared
    {
        get
        {
            Interlocked.Increment(ref _accessCount);

            if (_accessCount >= RotateAt)
            {
                _instance = NewRules();
            }

            return _instance;
        }
    }

    public Guid Salt { get; init; }

    public int HashHead { get; init; }

    public int HashTail { get; init; }

    public byte ChecksumStart { get; init; }

    public int[] ChecksumIndexes { get; init; } = null!;

    public UInt128 AppToken { get; init; }

    public string AppTokenDigest => AppToken.ToString("x16");

    public string HashFormat => $"{HashHead:x4}:{{0}}:{{1}}:{HashTail:x8}";

    public string? Sign(string? uri, string? userId, DateTime? timestamp)
    {
        if (string.IsNullOrEmpty(uri) || string.IsNullOrEmpty(userId) || !timestamp.HasValue)
            return null;

        timestamp = timestamp.Value.ToUniversalTime();

        var parsedUri = new Uri(uri);
        var parsedUrl = $"{parsedUri.AbsolutePath}{parsedUri.Query}";

        // 2023-08-05T20:52:11.288Z
        // to make this compliant with the javascript ISO format.
        var payload = $"{Salt}:{timestamp:yyyy-MM-dd'T'HH:mm:ss'.'fff'Z'}:{parsedUrl}:{userId}";

        byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(payload));
        string digest = string.Concat(hash.Select(b => b.ToString("x2")));

        byte[] digestBytes = Encoding.UTF8.GetBytes(digest);
        var sum = (int)ChecksumIndexes.Aggregate(ChecksumStart, (sum, idx) => (byte)(sum + digestBytes[idx]));
        sum = Math.Abs(sum);
        Console.WriteLine(sum);

        return string.Format(HashFormat, digest, $"{sum:D3}");
    }

    private static Rules NewRules()
    {
        return new Rules
        {
            Salt = Guid.NewGuid(),
            HashHead = RandomNumberGenerator.GetInt32(0x0, 0xffff),
            HashTail = RandomNumberGenerator.GetInt32(0x0, 0x7fffffff),
            ChecksumStart = RandomNumberGenerator.GetBytes(1).First(),
            ChecksumIndexes = Enumerable
                .Range(0, 32)
                .Select(_ => RandomNumberGenerator.GetInt32(0, 40))
                .ToArray(),
            AppToken = new UInt128(
                (ulong)Random.Shared.NextInt64(),
                (ulong)Random.Shared.NextInt64()),
        };
    }
}