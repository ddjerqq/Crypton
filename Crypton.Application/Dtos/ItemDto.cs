using Crypton.Domain.Entities;
using Crypton.Domain.ValueTypes;

namespace Crypton.Application.Dtos;

public sealed class ItemDto
{
    public Guid Id { get; set; }

    public ItemType ItemType { get; set; } = null!;

    public static implicit operator ItemDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            ItemType = item.ItemType,
        };
    }
}