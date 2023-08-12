using Crypton.Application.Interfaces;
using Crypton.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure.Persistence;

public static class ConfigureServices
{
    public static bool TryLoadConnectionString(out string connectionString)
    {
        connectionString = string.Empty;

        string GetEnv(string key, string? @default = null)
        {
            return Environment.GetEnvironmentVariable(key) ?? @default!;
        }

        var dbHost = GetEnv("POSTGRES_HOST", "localhost");
        var port = GetEnv("POSTGRES_PORT", "5432");
        var db = GetEnv("POSTGRES_DB", "postgres");
        var user = GetEnv("POSTGRES_USER", "postgres");

        var password = GetEnv("POSTGRES_PASSWORD");
        if (string.IsNullOrEmpty(password)) return false;

        var inDevelopment = GetEnv("DOTNET_ENVIRONMENT") == "Development"
                            || GetEnv("ASPNETCORE_ENVIRONMENT") == "Development";

        connectionString =
            $"Host={dbHost};" +
            $"Port={port};" +
            $"Database={db};" +
            $"Username={user};" +
            $"Password={password};" +
            $"Include Error Detail={inDevelopment}";

        return true;
    }

    public static IServiceCollection AddPersistenceServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<AuditableEntitySaveChangesInterceptor>();
        services.AddSingleton<UserMaterializationInterceptor>();

        services.AddDbContext<AppDbContext>(o =>
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is "Development")
            {
                o.EnableDetailedErrors();
                o.EnableSensitiveDataLogging();
            }

            if (TryLoadConnectionString(out var pgConnectionString))
            {
                o.UseNpgsql(pgConnectionString);
            }
            else
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                o.UseSqlite(connectionString);
            }
        });

        // delegate the IDbContext to the EmeraldDbContext;
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        return services;
    }
}