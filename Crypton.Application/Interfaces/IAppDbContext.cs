// <copyright file="IAppDbContext.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Entities;
using Crypton.Domain.ValueTypes;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Interfaces;

public interface IAppDbContext : IDisposable
{
    public DbSet<User> Users { get; }

    public DbSet<Item> Items { get; }

    public DbSet<Transaction> Transactions { get; }

    public DbSet<TransactionUser> TransactionUsers { get; }

    public DbSet<ItemType> ItemTypes { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default);
}