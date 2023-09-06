using Blazored.Toast;
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

// var baseUri = builder.HostEnvironment.BaseAddress;
var baseUri = "https://localhost/";
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseUri) });
builder.Services.AddBlazoredToast();

var host = builder.Build();

await host.RunAsync();