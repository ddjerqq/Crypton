using System.ComponentModel;
using NUnit.Framework;

namespace Crypton.Domain.Common.Extensions;

[EditorBrowsable(EditorBrowsableState.Never)]
internal class TestStringExtensions
{
    [Test]
    [Parallelizable]
    public void TestToSnakeCase()
    {
        var input = "TestString";
        var output = input.ToSnakeCase();

        Assert.That(output, Is.EqualTo("test_string"));
    }

    [Test]
    [Parallelizable]
    public void TestToHexBytesEven()
    {
        var hex = "ffff";
        var bytes = hex.ToHexBytes();
        Assert.That(bytes, Is.EqualTo(new byte[] { 255, 255 }));
    }

    [Test]
    [Parallelizable]
    public void TestToHexBytesOdd()
    {
        var hex = "fffff";
        var bytes = hex.ToHexBytes();
        Assert.That(bytes, Is.EqualTo(new byte[] { 255, 255, 15 }));
    }
}