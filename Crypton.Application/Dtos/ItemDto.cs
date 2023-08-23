using Crypton.Domain.Entities;

namespace Crypton.Application.Dtos;

public sealed class ItemDto
{
    public Guid Id { get; init; }

    public string TypeId { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public static implicit operator ItemDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            TypeId = item.ItemType.Id,
            TypeName = item.ItemType.Name,
            Price = item.ItemType.Price,
        };
    }
}