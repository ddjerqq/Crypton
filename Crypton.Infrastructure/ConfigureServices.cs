// <copyright file="ConfigureServices.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Interfaces;
using Crypton.Infrastructure.BackgroundServices;
using Microsoft.Extensions.DependencyInjection;

namespace Crypton.Infrastructure;

public static class ConfigureServices
{
    public const string CookieName = "authorization";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services)
    {
        services.AddHostedService<BlockChainWorker>();

        services.AddScoped<IBlockChainWorker>(sp =>
            sp.GetRequiredService<BlockChainWorker>());

        return services;
    }
}