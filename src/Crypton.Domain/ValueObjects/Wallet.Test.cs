using System.ComponentModel;
using Crypton.Domain.Common.Extensions;
using NUnit.Framework;

namespace Crypton.Domain.ValueObjects;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestWallet
{
    public const string PublicKey =
        "3059301306072a8648ce3d020106082a8648ce3d03010703420004b02d01b51cbef4eeb3ae7a7e0f538e430c85498326efd9eb2ae82e52ed207792b785f0c5db548f4c1c0e53fbe774fc9ab67f7301fc0d965120cbe19ee7bf82a6";

    public const string PrivateKey =
        "308187020100301306072a8648ce3d020106082a8648ce3d030107046d306b0201010420a23d32d2ddc3ec796aaaae46acfddadca0b97697f610589562d6c56f0b7e62d6a14403420004b02d01b51cbef4eeb3ae7a7e0f538e430c85498326efd9eb2ae82e52ed207792b785f0c5db548f4c1c0e53fbe774fc9ab67f7301fc0d965120cbe19ee7bf82a6";

    public const string Address = "ea53c2609f291826923e97e400f0a6d886a7dad8c5";

    public static readonly Wallet Wallet = Wallet.FromKeys(PublicKey, PrivateKey);

    [Test]
    [Parallelizable]
    public void TestNewWallets()
    {
        var wallet = Wallet.NewWallet();

        var pKeyDigest = wallet.PublicKey.HexDigest();
        var pKeyBytes = pKeyDigest.ToHexBytes();
        Assert.That(pKeyBytes, Is.EqualTo(wallet.PublicKey));

        var sKeyDigest = wallet.PrivateKey.HexDigest();
        var sKeyBytes = sKeyDigest.ToHexBytes();
        Assert.That(sKeyBytes, Is.EqualTo(wallet.PrivateKey));
    }

    [Test]
    [Parallelizable]
    public void TestWalletFromKeysByte()
    {
        var wallet = Wallet.FromKeys(PublicKey, PrivateKey);

        Assert.That(wallet.PublicKey, Is.EqualTo(PublicKey.ToHexBytes()));
        Assert.That(wallet.PrivateKey, Is.EqualTo(PrivateKey.ToHexBytes()));

        Assert.That(wallet.PublicKey, Is.EqualTo(Wallet.PublicKey));
        Assert.That(wallet.PrivateKey, Is.EqualTo(Wallet.PrivateKey));

        Assert.That(wallet.Address, Is.EqualTo(Address));
    }

    [Test]
    [Parallelizable]
    public void TestWalletFromKeysHex()
    {
        var wallet = Wallet.FromKeys(PublicKey.ToHexBytes(), PrivateKey.ToHexBytes());

        Assert.That(wallet.PublicKey, Is.EqualTo(PublicKey.ToHexBytes()));
        Assert.That(wallet.PrivateKey, Is.EqualTo(PrivateKey.ToHexBytes()));

        Assert.That(wallet.PublicKey, Is.EqualTo(Wallet.PublicKey));
        Assert.That(wallet.PrivateKey, Is.EqualTo(Wallet.PrivateKey));

        Assert.That(wallet.Address, Is.EqualTo(Address));
    }

    [Test]
    [Parallelizable]
    public void TestSign()
    {
        var payload = "aaa"u8.ToArray();
        var signature = Wallet.Sign(payload);
        Assert.That(Wallet.Verify(payload, signature), Is.True);
    }

    [Test]
    [Parallelizable]
    public void TestTwoWalletsHaveDifferentSignatures()
    {
        var payload = "aaa"u8.ToArray();
        var signature = Wallet.Sign(payload);

        var wallet = Wallet.NewWallet();

        Assert.That(Wallet.Verify(payload, signature), Is.True);
        Assert.That(wallet.Verify(payload, signature), Is.False);
    }

    [Test]
    [Parallelizable]
    public void TestGetHashCode()
    {
        Assert.That(Wallet.GetHashCode(), Is.EqualTo(Address.GetHashCode()));
    }
}