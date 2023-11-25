using System.Security.Cryptography;
using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Domain.ValueObjects;

public sealed record Wallet : ValueObjectBase
{
    private readonly ECDsa _ecdsa;

    private Wallet(ECDsa ecdsa) => _ecdsa = ecdsa;

    public byte[] PublicKey => _ecdsa.ExportSubjectPublicKeyInfo();
    public byte[] PrivateKey => _ecdsa.ExportPkcs8PrivateKey();

    public string Address => PrivateKey.Sha256().HexDigest()[22..64];

    public byte[] Sign(byte[] data) => _ecdsa.SignData(data, HashAlgorithmName.SHA512);

    public bool Verify(byte[] data, byte[] signature) => _ecdsa.VerifyData(data, signature, HashAlgorithmName.SHA512);

    public override int GetHashCode() => Address.GetHashCode();

    public static Wallet NewWallet() => new(ECDsa.Create(ECCurve.NamedCurves.nistP256));

    public static Wallet FromKeys(byte[] pKey, byte[] sKey)
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        ecdsa.ImportSubjectPublicKeyInfo(pKey, out _);
        ecdsa.ImportPkcs8PrivateKey(sKey, out _);

        return new Wallet(ecdsa);
    }

    public static Wallet FromKeys(string pKeyHex, string sKeyHex) => FromKeys(pKeyHex.ToHexBytes(), sKeyHex.ToHexBytes());
}