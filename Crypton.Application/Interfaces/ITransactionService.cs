using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface ITransactionService
{
    public IReadOnlyCollection<Transaction> Transactions { get; }

    public Task InitializeAsync(CancellationToken ct = default);

    public Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default);

    public decimal GetUserBalance(string userId);

    public IEnumerable<Item> GetUserItems(string userId);
}