using Crypton.Application.Interfaces;
using Crypton.Infrastructure.BackgroundServices;
using Crypton.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crypton.Infrastructure;

public static class ConfigureServices
{
    public const string CookieName = "authorization";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
    {
        services.AddSingleton<ITransactionWorker, TransactionWorker>();

        services.AddHostedService<TransactionWorker>(sp =>
            (TransactionWorker)sp.GetRequiredService<ITransactionWorker>());

        return services;
    }
}