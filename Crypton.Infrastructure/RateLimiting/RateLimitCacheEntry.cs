using System.Diagnostics.CodeAnalysis;

namespace Crypton.Infrastructure.RateLimiting;

internal sealed record RateLimitCacheEntry(int Remaining, TimeSpan Per)
{
    public int Remaining { get; private set; } = Remaining;

    public bool TryAcquire(out DateTime retryAfter)
    {
        retryAfter = default;

        if (Remaining > 0)
        {
            Remaining--;
            return true;
        }

        retryAfter = DateTime.UtcNow + Per;
        return false;
    }
}