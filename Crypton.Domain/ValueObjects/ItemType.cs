using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Entities;

namespace Crypton.Domain.ValueObjects;

public sealed record ItemType(string Id, string Name, decimal Price, float MinRarity, float MaxRarity)
    : ValueObjectBase
{
    public Item CreateItem(User owner)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Rarity = Rarity.CreateForItemType(this),
            ItemTypeId = this.Id,
            ItemType = this,
            OwnerId = owner.Id,
            Owner = owner,
        };
    }
}