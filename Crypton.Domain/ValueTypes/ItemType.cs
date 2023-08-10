using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.ValueTypes;

public sealed class ItemType : BaseAuditableDomainEntity, IComparable<ItemType>
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public float MinRarity { get; init; }

    public float MaxRarity { get; init; }

    public int CompareTo(ItemType? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        return this.Id.CompareTo(other.Id);
    }
}