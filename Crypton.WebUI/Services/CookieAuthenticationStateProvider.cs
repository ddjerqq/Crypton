using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Crypton.WebUI.Services;

public sealed class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;

    public CookieAuthenticationStateProvider(HttpClient http)
    {
        _http = http;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var claimsPrincipal = await GetUserStateAsync();
        return new AuthenticationState(claimsPrincipal);
    }

    public async Task LoginAsync( /* UserLoginCommand command */ CancellationToken ct = default)
    {
        var payload = new
        {
            username = "gloopmasta",
            password = "String123$",
            remember_me = true,
        };

        var result = await _http.PostAsJsonAsync("api/v1/auth/login", payload, ct);

        // TODO: handle error
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task<ClaimsPrincipal> GetUserStateAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/v1/auth/user_claims", ct);
        if (!resp.IsSuccessStatusCode)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var body = await resp.Content.ReadFromJsonAsync<Dictionary<string, string>>(ct);

        if (body is not { Count: > 0 })
            return new ClaimsPrincipal(new ClaimsIdentity());

        var claims = body
            .Select(kv => new Claim(kv.Key, kv.Value))
            .ToList();

        var claimsIdentity = new ClaimsIdentity(claims, "crypton");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return claimsPrincipal;
    }
}