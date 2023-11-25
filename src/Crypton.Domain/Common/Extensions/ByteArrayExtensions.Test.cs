using System.ComponentModel;
using NUnit.Framework;

namespace Crypton.Domain.Common.Extensions;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestByteArrayExtensions
{
    [Test]
    [Parallelizable]
    public void TestSha256Hash()
    {
        var hash = "aaa"u8.ToArray().Sha256().HexDigest();
        Console.WriteLine(hash);
        Assert.That(hash, Is.EqualTo("9834876dcfb05cb167a5c24953eba58c4ac89b1adf57f28f2f9d09af107ee8f0"));
    }

    [Test]
    [Parallelizable]
    public void TestHexDigest()
    {
        var hash = "aaa"u8.ToArray().Sha256().HexDigest();
        Console.WriteLine(hash);
        Assert.That(hash, Is.EqualTo("9834876dcfb05cb167a5c24953eba58c4ac89b1adf57f28f2f9d09af107ee8f0"));
    }

    [Test]
    [Parallelizable]
    public void TestToBase64String()
    {
        var hash = "aaa"u8.ToArray().Sha256().ToBase64String();
        Console.WriteLine(hash);
        Assert.That(hash, Is.EqualTo("mDSHbc+wXLFnpcJJU+uljErImxrfV/KPL50JrxB+6PA="));
    }

    [Test]
    [Parallelizable]
    public void TestToUtf8String()
    {
        var payload = "aaa"u8.ToArray();
        Console.WriteLine(payload.ToUtf8String());
        Assert.That(payload, Is.EqualTo("aaa"));
    }
}