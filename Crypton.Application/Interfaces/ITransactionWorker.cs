using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface ITransactionWorker
{
    public ValueTask EnqueueTransaction(User sender, User receiver, decimal amount, CancellationToken ct = default);

    public ValueTask EnqueueTransaction(User sender, User receiver, Item item, CancellationToken ct = default);
}