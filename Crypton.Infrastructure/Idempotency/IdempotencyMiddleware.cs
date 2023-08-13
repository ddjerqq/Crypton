using Crypton.Infrastructure.Services;
using Crypton.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure.Idempotency;

public sealed class IdempotencyMiddleware : IMiddleware
{
    private readonly IIdempotencyService idempotencyService;

    public IdempotencyMiddleware(IIdempotencyService idempotencyService)
    {
        this.idempotencyService = idempotencyService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        var attributes = endpoint?.Metadata.OfType<RequireIdempotencyAttribute>();
        var hasRequireIdempotency = attributes?.Any() ?? false;

        if (!hasRequireIdempotency)
        {
            await next(context);
            return;
        }

        var idempotencyKey = context.Request.Headers["X-Idempotency-Key"].FirstOrDefault();
        if (string.IsNullOrEmpty(idempotencyKey) || !Guid.TryParse(idempotencyKey, out var key))
        {
            context.Response.Headers.Clear();
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync("Invalid / missing idempotency key");

            return;
        }

        if (this.idempotencyService.ContainsKey(key))
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;

            await context.Response.WriteAsync("Conflicting idempotency key");

            return;
        }

        this.idempotencyService.AddKey(key);

        await next(context);
    }
}

public static class IdempotencyMiddlewareExtensions
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services)
    {
        services.AddScoped<IdempotencyMiddleware>();
        services.AddSingleton<IIdempotencyService, IdempotencyService>();

        return services;
    }

    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<IdempotencyMiddleware>();
    }
}