using System.Security.Cryptography;
using System.Text;

namespace Crypton.Domain.Common.Extensions;

public static class ByteArrayExtensions
{
    public static byte[] Sha256(this byte[] bytes) => SHA256.HashData(bytes);

    public static string HexDigest(this byte[] bytes) => BitConverter
        .ToString(bytes)
        .Replace("-", "")
        .ToLower();

    public static string ToBase64String(this byte[] bytes) => Convert.ToBase64String(bytes);


    public static string ToUtf8String(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
}
