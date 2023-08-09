// <copyright file="BlockchainService.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Application.Services;

public class BlockchainService : IBlockchainService
{
    private readonly List<Transaction> blocks;
    private readonly IServiceProvider services;

    public BlockchainService(IServiceProvider services)
    {
        this.services = services;

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        this.blocks = dbContext.Transactions
            .Include(x => x.Participants)
            .ToList();
    }

    public IReadOnlyCollection<Transaction> Blocks => this.blocks.AsReadOnly();

    public async Task AddBlockAsync(Transaction transaction, CancellationToken ct = default)
    {
        using var scope = this.services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        await dbContext.Transactions.AddAsync(transaction, ct);
        await dbContext.SaveChangesAsync(ct);

        this.blocks.Add(transaction);
    }

    public decimal GetUserBalance(string userId)
    {
        return this.blocks
            .OfType<BalanceTransaction>()
            .Where(x => x.Receiver.Id == userId || x.Sender.Id == userId)
            .Select(x => new
            {
                SenderId = x.Sender.Id,
                ReceiverId = x.Receiver.Id,
                x.Amount,
            })
            .Select(x => x.ReceiverId == userId ? x.Amount : -x.Amount)
            .Sum();
    }

    public IEnumerable<Item> GetUserItems(string userId)
    {
        var itemsReceived = this.blocks
            .OfType<ItemTransaction>()
            .Where(x => x.Receiver.Id == userId || x.Sender.Id == userId)
            .Select(x => new
            {
                SenderId = x.Sender.Id,
                ReceiverId = x.Receiver.Id,
                x.Item,
            })
            .AsEnumerable();

        var inventory = new Dictionary<Guid, Item>();

        foreach (var item in itemsReceived)
        {
            if (item.ReceiverId == userId)
                inventory[item.Item.Id] = item.Item;

            if (item.SenderId == userId)
                inventory.Remove(item.Item.Id);
        }

        return inventory.Values;
    }
}