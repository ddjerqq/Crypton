// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application;
using Crypton.Diamond;
using Crypton.Identity;
using Crypton.Infrastructure;
using Crypton.Persistence;
using Crypton.WebAPI;
using dotenv.net;

DotEnv.Fluent()
    .WithEnvFiles("./../.env")
    .Load();

// fix postgres issue with timestamps and DateTimes
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseStaticWebAssets();

builder.Services.AddSingleton(builder.Configuration);

builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddPersistenceServices()
    .AddWebApiServices(builder.Environment)
    .AddIdentityServices()
    .AddDigitalSignature();

var app = builder.Build();

app.MigrateDatabase()
    .ConfigureWebApiMiddleware()
    .Run();