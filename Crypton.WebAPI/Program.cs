using Crypton.Application;
using Crypton.Infrastructure;
using Crypton.Infrastructure.Diamond;
using Crypton.Infrastructure.Identity;
using Crypton.Infrastructure.Persistence;
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

// TODO: implement Rate limiting services
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices()
    .AddBackgroundServices()
    .AddPersistenceServices()
    .AddWebApiServices(builder.Environment)
    .AddIdentityServices()
    .AddDigitalSignature();

var app = builder.Build();

app.MigrateDatabase()
    .InitializeTransactions()
    .ConfigureWebApiMiddleware()
    .Run();