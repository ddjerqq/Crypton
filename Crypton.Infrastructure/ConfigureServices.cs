using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.BackgroundJobs;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Services;
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

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
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
}