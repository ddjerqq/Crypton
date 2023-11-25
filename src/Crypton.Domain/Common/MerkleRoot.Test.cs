using System.ComponentModel;
using System.Security.Cryptography;
using Crypton.Domain.Common.Extensions;
using NUnit.Framework;

namespace Crypton.Domain.Common;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestMerkleRoot
{
    [Test]
    [Parallelizable]
    public void TestMerkleRootOnPredeterminedHashes()
    {
        var txHashes = new List<byte[]>
        {
            "aaa"u8.ToArray().Sha256(),
            "bbb"u8.ToArray().Sha256(),
            "ccc"u8.ToArray().Sha256(),
            "ddd"u8.ToArray().Sha256(),
        };

        var merkleRootDigest = MerkleRoot
            .BuildMerkleRoot(txHashes)
            .HexDigest();

        Console.WriteLine(merkleRootDigest);

        var expected = "20d91ce8e5b46488788bee6b7b2dec6216168c5bf2e1dc484be420bad8462aa9";
        Assert.That(merkleRootDigest, Is.EqualTo(expected));
    }

    [Test]
    [Parallelizable]
    public void TestMerkleRootOnRandomTransactions()
    {
        var txHashes = new List<byte[]>();

        for (int i = 0; i < 100; i++)
        {
            var txHash = new byte[32];
            RandomNumberGenerator.Fill(txHash);
            txHashes.Add(txHash);
        }

        var merkleRootDigest = MerkleRoot
            .BuildMerkleRoot(txHashes)
            .HexDigest();

        Console.WriteLine($"Computed Merkel Root: {merkleRootDigest}");

        Assert.That(merkleRootDigest, Is.Not.Empty);
    }
}