using Crypton.Application.Interfaces;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.ModelBinders;
using Crypton.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddMvc(o => { o.ModelBinderProviders.Insert(0, new ModelBinderProvider()); });

        services.AddIdempotency();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
    {
        return services;
    }
}