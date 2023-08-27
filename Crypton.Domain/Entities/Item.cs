using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.ValueObjects;

namespace Crypton.Domain.Entities;

public sealed class Item : EntityBase
{
    public Guid Id { get; set; }

    public string ItemTypeId { get; set; } = string.Empty;

    public ItemType ItemType { get; init; } = null!;

    public Guid OwnerId { get; set; }

    public User Owner { get; init; } = null!;
}