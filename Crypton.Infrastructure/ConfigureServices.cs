using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}