﻿using System.Security.Claims;
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
        this.Rate = rate;
        this.Per = TimeSpan.FromSeconds(perSeconds);
        this.Key = GetDefaultKey;
    }

    public int Rate { get; }

    public TimeSpan Per { get; set; }

    public Func<HttpContext, CancellationToken, Task<string>> Key { get; set; }

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cache = context.HttpContext
            .RequestServices
            .GetRequiredService<IMemoryCache>();

        var key = await this.Key(context.HttpContext, context.HttpContext.RequestAborted);
        var cacheKey = $"cooldown:{key}";

        RateLimitCacheEntry? cacheEntry = await cache
            .GetOrCreateAsync(cacheKey, entry =>
            {
                entry.SetAbsoluteExpiration(this.Per);
                return Task.FromResult(new RateLimitCacheEntry(this.Rate, this.Per));
            });

        if (cacheEntry?.TryAcquire(out var retryAfter) ?? true)
        {
            await next();
        }
        else
        {
            context.Result = new StatusCodeResult(429);
            context.HttpContext.Response.Headers.Add("Retry-After", retryAfter.ToString("R"));
        }
    }

    private static Task<string> GetDefaultKey(HttpContext context, CancellationToken ct = default)
    {
        if (context.User is { Identity.IsAuthenticated: true, Claims: var claims })
        {
            var id = claims
                .First(x => x.Type == ClaimTypes.NameIdentifier)
                .Value;

            return Task.FromResult(id);
        }

        return Task.FromResult(context.Connection.Id);
    }
}