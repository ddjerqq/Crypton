using System.Security.Cryptography;

namespace Crypton.Domain.Common;

public static class MerkleRoot
{
    private static readonly byte[] Empty = Enumerable.Range(0, 32).Select(_ => (byte)0).ToArray();

    public static byte[] BuildMerkleRoot(List<byte[]> merkelLeaves)
    {
        if (!merkelLeaves.Any())
            return Empty;

        while (true)
        {
            if (merkelLeaves.Count == 1)
                return merkelLeaves.First();

            if (merkelLeaves.Count % 2 == 1)
                merkelLeaves.Add(merkelLeaves.Last());

            merkelLeaves = Enumerable.Range(0, merkelLeaves.Count - 1)
                .Where(i => i % 2 == 0)
                .Select(i => HashPair(merkelLeaves[i], merkelLeaves[i + 1]))
                .ToList();
        }
    }

    private static byte[] HashPair(byte[] left, byte[] right)
    {
        var buffer = new byte[left.Length + right.Length];

        left.CopyTo(buffer, 0);
        right.CopyTo(buffer, left.Length);

        return SHA256.HashData(buffer);
    }
}
