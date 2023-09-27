using Crypton.Domain.Common.Abstractions;

namespace Crypton.Domain.ValueObjects;

public sealed record DailyStreak : ValueObjectBase
{
    public DateTime DailyCollectedAt { get; private set; } = new(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// Gets a DateTime object representing the next time the user can collect the daily amount at in UTC
    /// </summary>
    public DateTime CollectNextDailyAfter => DailyCollectedAt.AddDays(1);

    public int Streak { get; private set; } = 0;

    /// <summary>
    /// if when we are collecting the daily amount, and the amount is 1 day old, reset the streak
    /// otherwise, increment the streak, until reaching 31 days
    /// </summary>
    public void CollectDaily()
    {
        Streak++;
        DailyCollectedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// find out if the user is eligible for daily coins
    /// </summary>
    /// <returns>true if the user is eligible, false otherwise</returns>
    public bool IsEligibleForDaily()
    {
        return DailyCollectedAt.AddDays(1) < DateTime.UtcNow;
    }
}