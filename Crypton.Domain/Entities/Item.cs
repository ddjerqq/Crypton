using Crypton.Domain.ValueObjects;

namespace Crypton.Domain.Entities;

public sealed class Item
{
    public Guid ItemTypeId { get; init; }

    public ItemType ItemType { get; init; } = null!;

    public Guid OwnerId { get; init; }

    public User Owner { get; init; } = null!;

    public Guid Id { get; set; }
}