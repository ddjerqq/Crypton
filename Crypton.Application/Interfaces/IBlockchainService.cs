// <copyright file="IBlockchainService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface IBlockchainService
{
    public IReadOnlyCollection<Transaction> Blocks { get; }

    public Task AddBlockAsync(Transaction transaction, CancellationToken ct = default);

    public decimal GetUserBalance(string userId);

    public IEnumerable<Item> GetUserItems(string userId);
}