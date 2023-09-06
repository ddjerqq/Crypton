using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure.RateLimiting;

[AttributeUsage(AttributeTargets.Method)]
public sealed class CooldownAttribute : ActionFilterAttribute
{
    public CooldownAttribute(int rate, double perSeconds)
    {
        Rate = rate;
        Per = TimeSpan.FromSeconds(perSeconds);
        Key = GetDefaultKey;
    }

    public int Rate { get; }

    public TimeSpan Per { get; set; }

    public Func<HttpContext, CancellationToken, Task<string>> Key { get; set; }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cache = context.HttpContext
            .RequestServices
            .GetRequiredService<IMemoryCache>();

        var uri = context.HttpContext.Request.Path.Value;
        var key = await Key(context.HttpContext, context.HttpContext.RequestAborted);
        var cacheKey = $"cooldown:{uri}:{key}";

        RateLimitCacheEntry? cacheEntry = await cache
            .GetOrCreateAsync(cacheKey, entry =>
            {
                entry.SetAbsoluteExpiration(Per);
                return Task.FromResult(new RateLimitCacheEntry(Rate, Per));
            });

        if (cacheEntry?.TryAcquire(out var retryAfter) ?? true)
        {
            await next();
            return;
        }

        context.Result = new StatusCodeResult(429);
        context.HttpContext.Response.Headers.Add("Retry-After", retryAfter.ToString("R"));
    }

    private static Task<string> GetDefaultKey(HttpContext context, CancellationToken ct = default)
    {
        var key = context.Connection.RemoteIpAddress?.ToString() ?? context.Connection.Id;

        if (context.User is { Identity.IsAuthenticated: true, Claims: var claims })
        {
            var id = claims
                .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
                .Value;

            if (!string.IsNullOrEmpty(id))
                key = id;
        }

        return Task.FromResult(key);
    }
}