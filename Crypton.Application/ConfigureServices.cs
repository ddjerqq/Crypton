using System.Reflection;
using Crypton.Application.Common.Behaviours;
using Crypton.Domain;
using FluentValidation;
using MediatR;

namespace Crypton.Application;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(DomainAssembly.Assembly);
        services.AddValidatorsFromAssembly(ApplicationAssembly.Assembly);

        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(ApplicationAssembly.Assembly); });
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ErrorHandlingBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehaviour<,>));
        });

        return services;
    }
}