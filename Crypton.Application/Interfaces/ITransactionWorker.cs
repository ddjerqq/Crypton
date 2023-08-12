using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface ITransactionWorker
{
    // TODO make this return transaction Ids so that we can keep track of them
    public ValueTask EnqueueTransaction(User sender, User receiver, decimal amount, CancellationToken ct = default);

    // TODO make this return transaction Ids so that we can keep track of them
    public ValueTask EnqueueTransaction(User sender, User receiver, Item item, CancellationToken ct = default);
}