using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Crypton.Application;
using Crypton.WebUI;
using Crypton.WebUI.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>(sp =>
    sp.GetRequiredService<CookieAuthenticationStateProvider>());

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// digital signature rules
// idempotency handler
// error handler
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddBlazoredLocalStorageAsSingleton(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = new SnakeCaseJsonNamingPolicy();
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddBlazorBootstrap();

var host = builder.Build();

await host.RunAsync();