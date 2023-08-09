// <copyright file="BlockChainWorker.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Interfaces;
using Crypton.Domain.Common;
using Crypton.Domain.Entities;
using Microsoft.Extensions.Hosting;

namespace Crypton.Infrastructure.BackgroundServices;

public class BlockChainWorker : BackgroundService, IBlockChainWorker
{
    private readonly IBlockchainService blockchain;

    private readonly AsyncQueue<Transaction> queue = new();
    private readonly SemaphoreSlim processingSemaphore = new(1);

    public BlockChainWorker(IBlockchainService blockchain)
    {
        this.blockchain = blockchain;
    }

    private Transaction LastTransaction => this.blockchain.Blocks
        .OrderByDescending(x => x.Index)
        .First();

    public bool TryEnqueueTransaction(User sender, User receiver, decimal amount)
    {
        if (sender.Balance < amount)
            return false;

        var block = this.LastTransaction.Next(sender, receiver, amount);
        this.queue.Enqueue(block);

        return true;
    }

    public bool TryEnqueueTransaction(User sender, User receiver, Item item)
    {
        if (sender.Items.All(x => x.Id != item.Id))
            return false;

        var block = this.LastTransaction.Next(sender, receiver, item);
        this.queue.Enqueue(block);

        return true;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await foreach (var block in this.queue.WithCancellation(ct))
            {
                try
                {
                    await this.processingSemaphore.WaitAsync(ct);

                    await block.Mine(ct);
                    await this.blockchain.AddBlockAsync(block, ct);
                }
                finally
                {
                    this.processingSemaphore.Release();
                }
            }
        }
    }
}