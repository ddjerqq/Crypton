// <copyright file="Item.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.ValueTypes;

namespace Crypton.Domain.Entities;

public sealed class Item
{
    public Guid Id { get; init; }

    public Guid ItemTypeId { get; init; }

    public ItemType ItemType { get; init; } = null!;

    public string OwnerId { get; init; } = string.Empty;

    public User Owner { get; init; } = null!;
}