using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Entities;

namespace Crypton.Domain.ValueObjects;

public interface IInventory : IList<Item>, IValueObject
{
    bool HasItemWithId(Guid itemId);

    void Transfer(IInventory other, Item item)
    {
        this.Remove(item);
        other.Add(item);
    }
}

public sealed class Inventory : List<Item>, IInventory
{
    public bool HasItemWithId(Guid itemId)
    {
        return this.Any(x => x.Id == itemId);
    }
}