using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.BackgroundJobs;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.RateLimiting;
using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Crypton.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
        services.AddIdempotency();

        services.AddMemoryCache();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
    {
        services.AddQuartz(config =>
        {
            var jobKey = new JobKey("ProcessOutboxMessagesJob");
            config
                .AddJob<ProcessOutboxMessagesBackgroundJob>(jobKey)
                .AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .WithSimpleSchedule(schedule => schedule
                        .WithInterval(TimeSpan.FromSeconds(10))
                        .RepeatForever()));
        });

        services.AddQuartzHostedService();

        return services;
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var policies = RateLimitConstants.LoadRateLimitOptions(configuration)
            .ToList();

        var globalPolicy = policies
            .First(x => x.PolicyName == RateLimitConstants.GlobalPolicyName);

        services.AddRateLimiter(rateLimitOptions =>
        {
            rateLimitOptions.RejectionStatusCode = 429;

            rateLimitOptions.OnRejected = (ctx, _) =>
            {
                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    ctx.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds)
                        .ToString(NumberFormatInfo.InvariantInfo);
                }

                return ValueTask.CompletedTask;
            };

            rateLimitOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
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

                return RateLimitPartition.GetTokenBucketLimiter(key, _ => globalPolicy);
            });
        });

        return services;
    }
}