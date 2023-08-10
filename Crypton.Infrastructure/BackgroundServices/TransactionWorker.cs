using System.Threading.Channels;
using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Crypton.Infrastructure.BackgroundServices;

public class TransactionWorker : BackgroundService, ITransactionWorker
{
    private readonly ITransactionService transactionService;
    private readonly ILogger<TransactionWorker> logger;

    private readonly Channel<QueueItem> channel;
    private readonly SemaphoreSlim processingSemaphore = new(1);
    private readonly SemaphoreSlim queueSemaphore = new(1);

    private long index;

    public TransactionWorker(
        ITransactionService transactionService,
        ILogger<TransactionWorker> logger)
    {
        this.channel = Channel.CreateUnbounded<QueueItem>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        });

        this.transactionService = transactionService;
        this.logger = logger;
    }

    public async ValueTask EnqueueTransaction(
        User sender,
        User receiver,
        decimal amount,
        CancellationToken ct = default)
    {
        try
        {
            await this.queueSemaphore.WaitAsync(ct);

            var queueItem = new QueueItem
            {
                Index = this.index,
                Sender = sender,
                Receiver = receiver,
                Amount = amount,
                Item = null,
            };

            // double safety mechanism, redundant or just stupid?
            Interlocked.Increment(ref this.index);

            await this.channel.Writer.WriteAsync(queueItem, ct);
        }
        finally
        {
            this.queueSemaphore.Release();
        }
    }

    public async ValueTask EnqueueTransaction(
        User sender,
        User receiver,
        Item item,
        CancellationToken ct = default)
    {
        try
        {
            await this.queueSemaphore.WaitAsync(ct);

            var queueItem = new QueueItem
            {
                Index = this.index,
                Sender = sender,
                Receiver = receiver,
                Amount = null,
                Item = item,
            };

            // double safety mechanism, redundant or just stupid?
            Interlocked.Increment(ref this.index);

            await this.channel.Writer.WriteAsync(queueItem, ct);
        }
        finally
        {
            this.queueSemaphore.Release();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        this.index = this.transactionService.Transactions
            .OrderByDescending(x => x.Index)
            .First().Index + 1;

        while (!ct.IsCancellationRequested)
        {
            var isDataAvailable = await this.channel.Reader.WaitToReadAsync(ct);
            if (!isDataAvailable) break;

            var queueItem = await this.channel.Reader.ReadAsync(ct);

            try
            {
                var transactionBefore = this.transactionService.Transactions
                    .First(x => x.Index == queueItem.Index - 1);

                var transaction = queueItem.Amount is not null
                    ? transactionBefore.Next(queueItem.Sender, queueItem.Receiver, queueItem.Amount!.Value)
                    : transactionBefore.Next(queueItem.Sender, queueItem.Receiver, queueItem.Item!);

                this.logger.LogInformation(
                    "Received new transaction to mine: {Transaction}",
                    transaction);

                await this.processingSemaphore.WaitAsync(ct);

                await transaction.Mine(ct);

                await this.transactionService.AddTransactionAsync(transaction, ct);
            }
            finally
            {
                this.processingSemaphore.Release();
            }
        }
    }
}

public sealed class QueueItem
{
    public long Index { get; set; }

    public User Sender { get; set; } = null!;

    public User Receiver { get; set; } = null!;

    public decimal? Amount { get; set; }

    public Item? Item { get; set; }
}