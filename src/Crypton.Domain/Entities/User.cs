using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.ValueObjects;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    public Wallet Wallet { get; init; } = Wallet.NewWallet();

    public Inventory Inventory { get; init; } = new();

    public DailyStreak DailyStreak { get; init; } = new();
}