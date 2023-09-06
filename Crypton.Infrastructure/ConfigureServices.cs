using System.Globalization;
using System.Net;
using System.Threading.RateLimiting;
using Crypton.Application.Caching;
using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.BackgroundJobs;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Policies;
using Crypton.Infrastructure.RateLimiting;
using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
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
        services.AddOutputCache(options =>
        {
            options.DefaultExpirationTimeSpan = TimeSpan.FromMinutes(5);

            options.AddBasePolicy(IgnoreAuthCachePolicy.Instance);

            options.AddPolicy(CacheConstants.AllItemTypesPolicyName, policy =>
            {
                policy
                    .AddPolicy<IgnoreAuthCachePolicy>()
                    .SetVaryByHeader(HeaderNames.Authorization)
                    .Expire(TimeSpan.FromHours(1))
                    .Tag(CacheConstants.AllItemTypesPolicyName);
            });
        });

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

            rateLimitOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress is not null && !IPAddress.IsLoopback(remoteIpAddress))
                {
                    return RateLimitPartition.GetTokenBucketLimiter(remoteIpAddress!, _ => globalPolicy);
                }

                return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
            });
        });

        return services;
    }
}