using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface ITransactionService
{
    public IReadOnlyCollection<Transaction> Transactions { get; }

    public Task InitializeAsync(CancellationToken ct = default);

    public Task AddTransactionAsync(Transaction transaction, CancellationToken ct = default);

    public decimal GetUserBalance(Guid userId);

    public IEnumerable<Item> GetUserItems(Guid userId);
}