using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Crypton.Application;
using Crypton.Application.Common.Interfaces;
using Crypton.WebUIOld;
using Crypton.WebUIOld.HttpMessageHandlers;
using Crypton.WebUIOld.Services;
using Crypton.WebUIOld.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// run debug chrome
// chrome --remote-debugging-port=9222 --user-data-dir="C:\Users\gio20\AppData\Local\Temp\blazor-chrome-debug" https://localhost

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CookieAuthenticationStateProvider>());

builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

builder.Services.AddScoped<IThemeService, ThemeService>();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<HttpClient>(sp =>
{
    return new HttpClient() { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };

    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    var currentUserAccessor = sp.GetRequiredService<ICurrentUserAccessor>();

    var pipeline = new LoggingMessageHandler(loggerFactory)
    {
        InnerHandler = new RateLimitedMessageHandler(loggerFactory)
        {
            InnerHandler = new DigitalSignatureMessageHandler(currentUserAccessor)
            {
                InnerHandler = new IdempotentMessageHandler(loggerFactory)
                {
                    InnerHandler = new HttpClientHandler(),
                },
            },
        },
    };

    return new HttpClient(pipeline)
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
        Timeout = TimeSpan.FromSeconds(3),
    };
});

builder.Services.AddBlazoredLocalStorageAsSingleton(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddBlazorBootstrap();

var host = builder.Build();

await host.RunAsync();