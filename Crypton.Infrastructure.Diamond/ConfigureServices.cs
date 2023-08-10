using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure.Diamond;

public static class ConfigureServices
{
    public static IServiceCollection AddDigitalSignature(this IServiceCollection services)
    {
        services.AddSingleton<IRules>(Rules.Random());

        services.AddScoped<DigitalSignatureMiddleware>();
        services.AddScoped<IValidator<RulePayload>, RulePayloadValidator>();

        return services;
    }

    public static WebApplication UseDigitalSignature(this WebApplication app)
    {
        app.UseMiddleware<DigitalSignatureMiddleware>();

        return app;
    }
}