using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.ValueObjects;

public sealed record DailyStreak : ValueObjectBase
{
    public DateTime DailyCollectedAt { get; private set; } = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public DateTime CollectNextDailyAt => this.DailyCollectedAt.AddDays(1);

    public int Streak { get; private set; } = 0;

    /// <summary>
    /// if when we are collecting the daily amount, and the amount is 1 day old, reset the streak
    /// otherwise, increment the streak, until reaching 31 days
    /// </summary>
    public void CollectDaily()
    {
        this.Streak++;
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
}