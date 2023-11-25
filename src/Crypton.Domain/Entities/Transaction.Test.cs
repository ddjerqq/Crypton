using System.ComponentModel;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.ValueObjects;
using NUnit.Framework;

namespace Crypton.Domain.Entities;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestTransaction
{
    [Test]
    [Parallelizable]
    public void TestTransactionTimestamp()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var transaction = new Transaction(sender, receiver, 10, 1, timestamp);

        Console.WriteLine(transaction.Timestamp);

        Assert.That(transaction.Timestamp, Is.EqualTo(DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime));
    }

    [Test]
    [Parallelizable]
    public void TestTransactionHash()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var transaction = new Transaction(sender, receiver, 10, 1, timestamp);

        var hash = transaction.Hash.HexDigest();

        Console.WriteLine(hash);
        Assert.That(hash, Is.Not.Null);
    }

    [Test]
    [Parallelizable]
    public void TestTransactionSignature()
    {
        var sender = Wallet.NewWallet();
        var receiver = Wallet.NewWallet();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var transaction = new Transaction(sender, receiver, 10, 1, timestamp);

        Console.WriteLine($"Transaction Hash: {transaction.Hash.HexDigest()}");
        Assert.That(transaction.Hash.HexDigest(), Is.Not.Null);

        Console.WriteLine($"Transaction Sender Signature: {transaction.Signature.ToBase64String()}");
        Assert.That(transaction.Signature.ToBase64String(), Is.Not.Null);

        var isValid = transaction.Sender.Verify(transaction.Payload, transaction.Signature);
        Assert.That(isValid, Is.True);
    }
}