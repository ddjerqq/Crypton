using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.ValueObjects;

public sealed record Wallet(decimal Balance = default) : ValueObjectBase
{
    public decimal Balance { get; private set; } = Balance;

    public bool HasBalance(decimal amount)
    {
        return this.Balance >= amount;
    }

    public void Transfer(Wallet other, decimal amount)
    {
        if (!this.HasBalance(amount))
            throw new InvalidOperationException("Insufficient funds.");

        this.Balance -= amount;
        other.Balance += amount;
    }
}