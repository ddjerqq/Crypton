using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Entities;

namespace Crypton.Domain.Events;

public sealed record ItemSoldEvent
    : IDomainEvent
{
    public ItemSoldEvent(Item item)
    {
        Item = new Item
        {
            Id = item.Id,
            Rarity = item.Rarity,

            ItemTypeId = item.ItemTypeId,

            // clone                 vvvvvvvv
            ItemType = item.ItemType with { },

            OwnerId = Guid.Empty,
            Owner = null!,
        };
    }

    public DateTime SoldAt { get; set; } = DateTime.UtcNow;

    public Item Item { get; init; }
}