namespace Crypton.Infrastructure.Diamond;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class IgnoreDigitalSignatureAttribute : Attribute
{
}