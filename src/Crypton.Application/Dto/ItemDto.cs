﻿using Crypton.Domain.Entities;

namespace Crypton.Application.Dto;

public sealed record ItemDto
{
    public Guid Id { get; init; }

    public float Rarity { get; init; }

    public string TypeId { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public decimal TypePrice { get; init; }

    public float TypeMinRarity { get; init; }

    public float TypeMaxRarity { get; init; }

    public decimal Price { get; init; }

    public static implicit operator ItemDto(Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            Rarity = item.Rarity.Value,
            TypeId = item.ItemType.Id,
            TypeName = item.ItemType.Name,
            TypePrice = item.ItemType.Price,
            TypeMinRarity = item.ItemType.MinRarity,
            TypeMaxRarity = item.ItemType.MaxRarity,
            Price = item.Price,
        };
    }
}