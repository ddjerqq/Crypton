using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Services;

// TODO: turn this into a scoped service, with no
// in-memory cache of the blocks
public sealed class TransactionService : ITransactionService
{
    private readonly IServiceProvider services;
    private readonly List<Transaction> transactions = new();

    public TransactionService(IServiceProvider services)
    {
        this.services = services;
    }

    public IReadOnlyCollection<Transaction> Transactions => this.transactions.AsReadOnly();

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        using var scope = this.services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        // check system user
        var systemUserCount = await dbContext.Users
            .CountAsync(x => x.Id == GuidExtensions.ZeroGuid, ct);

        if (systemUserCount == 0)
        {
            await dbContext.Users.AddAsync(User.SystemUser(), ct);
            await dbContext.SaveChangesAsync(ct);
        }

        // check genesis block
        var blockCount = await dbContext.Transactions
            .CountAsync(x => x.Id == GuidExtensions.ZeroGuid, ct);

        if (blockCount == 0)
        {
            var transaction = Transaction.Genesis();
            await dbContext.Transactions.AddAsync(transaction, ct);
            await dbContext.SaveChangesAsync(ct);
        }

        var transactionEnumerable = await dbContext.Transactions
            .Include(x => x.Participants)
            .ThenInclude(x => x.User)
            .OrderBy(x => x.Index)
            .ToListAsync(ct);

        foreach (var transaction in transactionEnumerable)
        {
            if (!transaction.IsValid)
                throw new Exception($"Blockchain modified. transaction at index: {transaction.Index} is invalid");

            this.transactions.Add(transaction);
        }
    }

    public async Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default)
    {
        using var scope = this.services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TransactionService>>();

        this.transactions.Add(transaction);

        dbContext.Entry(transaction.Participants.ElementAt(0).User).State = EntityState.Unchanged;
        dbContext.Entry(transaction.Participants.ElementAt(1).User).State = EntityState.Unchanged;

        await dbContext.Transactions.AddAsync(transaction, ct);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "Transaction with id: {TransactionId} has been successfully minted on the blockchain",
            transaction.Id);
    }

    public decimal GetUserBalance(Guid userId)
    {
        return this.transactions
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

    public IEnumerable<Item> GetUserItems(Guid userId)
    {
        var itemsReceived = this.transactions
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