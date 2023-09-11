using System.Reflection;
using Crypton.WebAPI.OperationFilters;
using Microsoft.OpenApi.Models;

namespace Crypton.WebAPI;

public static class ConfigureSwagger
{
    public static IServiceCollection AddSwagger(this IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SupportNonNullableReferenceTypes();

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Crypton.WebAPI",
                Version = "v1",
                Description = "Crypton Web API",
                Contact = new OpenApiContact
                {
                    Name = "Crypton",
                    Email = "ddjerqq@gmail.com",
                    Url = new Uri("https://github.com/ddjerqq"),
                },
            });

            c.ResolveConflictingActions(x => x.First());
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme.",
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    new string[] { }
                },
            });

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            // for idempotency keys
            c.OperationFilter<IdempotencyKeyOperationFilter>();

            // for default responses
            c.OperationFilter<DefaultResponseOperationFilter>();
        });

        return services;
    }
}