using System.Diagnostics.CodeAnalysis;

namespace Crypton.Infrastructure.RateLimiting;

internal sealed record RateLimitCacheEntry(int Remaining, TimeSpan Per)
{
    public int Remaining { get; private set; } = Remaining;

    public bool TryAcquire([MaybeNullWhen(true)] [NotNullWhen(false)] out DateTime retryAfter)
    {
        retryAfter = default;

        if (this.Remaining > 0)
        {
            this.Remaining--;
            return true;
        }

        retryAfter = DateTime.UtcNow + this.Per;
        return false;
    }
}