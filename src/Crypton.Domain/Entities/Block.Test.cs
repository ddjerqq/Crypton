using System.ComponentModel;
using Crypton.Domain.Common;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.ValueObjects;
using NUnit.Framework;

namespace Crypton.Domain.Entities;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestBlock
{
    [Test]
    [Parallelizable]
    public void TestBlockHash()
    {
        var block = new Block(4, 0, 0, new string('0', 64).ToHexBytes());
        var hash = block.Hash.HexDigest();
        Console.WriteLine(hash);
    }

    [Test]
    [Parallelizable]
    public void TestBlockHashChangesWhenNonceIsChanged()
    {
        var block = new Block(4, 0, 0, new string('0', 64).ToHexBytes());
        var hash1 = block.Hash.HexDigest();
        block.Nonce++;
        var hash2 = block.Hash.HexDigest();

        Console.WriteLine(hash1);
        Console.WriteLine(hash2);

        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    [Parallelizable]
    public void TestBlockMine()
    {
        // 1 difficulty means 2 leading zeroes
        var block = new Block(3, 0, 0, new string('0', 64).ToHexBytes());

        block.Mine();

        Console.WriteLine(block.Nonce);
        Console.WriteLine(block.Hash.HexDigest());

        Assert.That(block.IsHashValid, Is.True);
    }

    [Test]
    [Parallelizable]
    public void TestBlockMineParallel()
    {
        // 1 difficulty means 2 leading zeroes
        var block = new Block(3, 0, 0, new string('0', 64).ToHexBytes());

        block.Mine();

        Console.WriteLine(block.Nonce);
        Console.WriteLine(block.Hash.HexDigest());

        Assert.That(block.IsHashValid, Is.True);
    }

    [Test]
    [Parallelizable]
    public void TestCalculateMerkleRootForTransactions()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var block = new Block(2, 0, 0, new string('0', 64).ToHexBytes());

        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));

        Console.WriteLine(block.MerkleRoot.HexDigest());
    }

    [Test]
    [Parallelizable]
    public void TestHashBlockWithTransactions()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var block = new Block(2, 0, 0, new string('0', 64).ToHexBytes());

        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 10, 1, timestamp));

        Console.WriteLine(block.MerkleRoot.HexDigest());

        // mine operation
        block.Mine();

        Console.WriteLine(block.Nonce);
        Console.WriteLine(block.Hash.HexDigest());

        Assert.That(block.IsHashValid, Is.True);
    }

    [Test]
    [Parallelizable]
    public void TestFeeCalculationIsCorrect()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var block = new Block(2, 0, 0, new string('0', 64).ToHexBytes());
        block.AddTransaction(new Transaction(sender, receiver, 1, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 1, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 1, 1, timestamp));
        block.AddTransaction(new Transaction(sender, receiver, 1, 1, timestamp));

        Assert.That(block.TotalFee, Is.EqualTo(4));
    }

    [Test]
    [Parallelizable]
    public void TestBlockRaisesArgumentExceptionWhenTransactionIsInvalid()
    {
        var sender = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var block = new Block(2, 0, 0, new string('0', 64).ToHexBytes());

        var invalidTransaction = new Transaction(sender, sender, 1, 1, timestamp);

        Assert.That(() => { block.AddTransaction(invalidTransaction); }, Throws.ArgumentException);
    }
}