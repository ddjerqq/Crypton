using System.ComponentModel.DataAnnotations.Schema;
using Crypton.Domain.Common.Abstractions;
using Crypton.Domain.Common.Extensions;

namespace Crypton.Domain.Entities;

public sealed class User : UserBase
{
    public DateTime DailyCollectedAt { get; private set; } = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [NotMapped]
    public DateTime CollectNextDailyAt => this.DailyCollectedAt.AddDays(1);

    public int DailyStreak { get; private set; } = 0;

    [NotMapped]
    public decimal Balance { get; private set; }

    [NotMapped]
    public IReadOnlyCollection<Item> Items { get; private set; } = new List<Item>();

    public bool IsSystem => this.Id == GuidExtensions.ZeroGuid;

    public static User SystemUser()
    {
        return new User
        {
            Id = GuidExtensions.ZeroGuid,
            UserName = "system",
            NormalizedUserName = "SYSTEM",
            Email = "system@crypton.com",
            NormalizedEmail = "SYSTEM@CRYPTON.COM",
        };
    }

    /// <summary>
    /// if when we are collecting the daily amount, and the amount is 1 day old, reset the streak
    /// otherwise, increment the streak, until reaching 31 days
    /// </summary>
    public void CollectDaily()
    {
        this.DailyStreak++;
        this.DailyCollectedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// find out if the user is eligible for daily coins
    /// </summary>
    /// <returns>true if the user is eligible, false otherwise</returns>
    public bool IsEligibleForDaily()
    {
        return this.DailyCollectedAt.AddDays(1) < DateTime.UtcNow;
    }

    /// <summary>
    /// set balance for this user. this should only be used
    /// when loading the user balance from transactions
    /// </summary>
    /// <param name="balance">the user's balance</param>
    public void SetBalance(decimal balance)
    {
        this.Balance = balance;
    }

    /// <summary>
    /// set items for this user. this should only be used
    /// when loading the user items from transactions
    /// </summary>
    /// <param name="items">the user's items</param>
    public void SetItems(IEnumerable<Item> items)
    {
        this.Items = items.ToList().AsReadOnly();
    }
}