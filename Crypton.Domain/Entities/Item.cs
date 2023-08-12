using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.ValueTypes;

namespace Crypton.Domain.Entities;

public sealed class Item
{
    public Guid Id { get; init; }

    public Guid ItemTypeId { get; init; }

    public ItemType ItemType { get; init; } = null!;

    [NotMapped]
    public Guid OwnerId { get; init; }

    [NotMapped]
    public User Owner { get; init; } = null!;
}