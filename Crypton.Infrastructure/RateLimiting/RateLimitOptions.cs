using System.Threading.RateLimiting;

namespace Crypton.Infrastructure.RateLimiting;

public sealed class RateLimitOptions
{
    public string PolicyName { get; set; } = string.Empty;

    public int ReplenishmentPeriod { get; set; } = 1;

    public int QueueLimit { get; set; } = 20;

    public int TokenLimit { get; set; } = 100;

    public int TokensPerPeriod { get; set; } = 10;

    public bool AutoReplenishment { get; set; } = true;

    public static implicit operator TokenBucketRateLimiterOptions(RateLimitOptions options)
    {
        return new TokenBucketRateLimiterOptions
        {
            AutoReplenishment = options.AutoReplenishment,
            QueueLimit = options.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            ReplenishmentPeriod = TimeSpan.FromSeconds(options.ReplenishmentPeriod),
            TokenLimit = options.TokenLimit,
            TokensPerPeriod = options.TokensPerPeriod,
        };
    }
}