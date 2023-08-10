namespace Crypton.Domain.Common.Extensions;

public static class GuidExtensions
{
    public const string ZeroGuidValue = "00000000-0000-0000-0000-000000000000";
    public static readonly Guid ZeroGuid = Guid.Parse(ZeroGuidValue);
}