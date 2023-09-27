namespace Crypton.Infrastructure.Diamond;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class IgnoreDigitalSignatureAttribute : Attribute
{
}