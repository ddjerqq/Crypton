// <copyright file="ConfigureServices.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.Persistence.Interceptors;
using Microsoft.EntityFrameworkCore;
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

        string dbHost = GetEnv("POSTGRES_HOST", "localhost");
        string port = GetEnv("POSTGRES_PORT", "5432");
        string db = GetEnv("POSTGRES_DB", "postgres");
        string user = GetEnv("POSTGRES_USER", "postgres");

        string password = GetEnv("POSTGRES_PASSWORD");
        if (string.IsNullOrEmpty(password)) return false;

        bool inDevelopment = GetEnv("DOTNET_ENVIRONMENT") == "Development"
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

    public static IServiceCollection AddPersistenceServices(this IServiceCollection services)
    {
        services.AddDbContextPool<AppDbContext>(o =>
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") is "Development")
            {
                o.EnableDetailedErrors();
                o.EnableSensitiveDataLogging();
            }

            if (TryLoadConnectionString(out string connectionString))
                o.UseNpgsql(connectionString);
            else
                o.UseSqlite("Data Source=C:/work/crypton/app.db;");

            o.AddInterceptors(new AuditableEntitySaveChangesInterceptor());
        });

        // delegate the IDbContext to the EmeraldDbContext;
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>();

        return services;
    }
}