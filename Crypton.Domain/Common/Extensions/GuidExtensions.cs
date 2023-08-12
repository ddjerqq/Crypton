namespace Crypton.Domain.Common.Extensions;

public static class GuidExtensions
{
    public const string ZeroGuidValue = "11111111-1111-1111-1111-111111111111";
    public static readonly Guid ZeroGuid = Guid.Parse(ZeroGuidValue);
}