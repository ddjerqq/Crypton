// <copyright file="User.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    [NotMapped]
    public decimal Balance { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<Item> Items { get; private set; } = new List<Item>();

    public void SetBalance(decimal balance)
    {
        this.Balance = balance;
    }

    public void SetItems(IEnumerable<Item> items)
    {
        this.Items = items.ToList().AsReadOnly();
    }
}