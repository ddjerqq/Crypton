using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.ValueObjects;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    public Wallet Wallet { get; init; } = new();

    public Inventory Inventory { get; init; } = new();

    public DailyStreak DailyStreak { get; init; } = new();
}