namespace Crypton.Infrastructure.Services.RateLimiting;

public sealed class TokenBucketRateLimitOptions
{
    public const string RateLimitConfigSection = "TokenBucketRateLimitOptions";

    public const string PolicyName = "TokenBucketRateLimit";

    public int ReplenishmentPeriod { get; set; } = 1;

    public int QueueLimit { get; set; } = 20;

    public int TokenLimit { get; set; } = 10;

    public int TokensPerPeriod { get; set; } = 5;

    public bool AutoReplenishment { get; set; } = true;
}