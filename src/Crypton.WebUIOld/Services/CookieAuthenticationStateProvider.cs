using System.Net.Http.Json;
using System.Security.Claims;
using Crypton.Application.Auth.Commands;
using Microsoft.AspNetCore.Components.Authorization;

namespace Crypton.WebUIOld.Services;

public sealed class CookieAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _http;
    private AuthenticationState _authenticationState;

    public CookieAuthenticationStateProvider(HttpClient http)
    {
        _http = http;
        _authenticationState = new AuthenticationState(new ClaimsPrincipal());
    }

    /// <summary>
    /// Gets or sets the AuthenticationState. When setting a new state,
    /// if the user is not authenticated, the state is not updated.
    /// </summary>
    public AuthenticationState AuthenticationState
    {
        get => _authenticationState;
        set
        {
            // TODO: this may cause issues when we are logging out.
            // we should make a separate method for that then.
            if (value.User.Identity?.IsAuthenticated ?? false)
                return;

            _authenticationState = value;
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_authenticationState.User.Identity?.IsAuthenticated ?? false)
            return _authenticationState;

        var claimsPrincipal = await FetchClaimsAsync();
        AuthenticationState = new AuthenticationState(claimsPrincipal);

        return AuthenticationState;
    }

    /// <summary>
    /// fetch claims from the server, and if the operation is successful,
    /// update the <see cref="AuthenticationState"/>
    /// </summary>
    public async Task RefreshClaims(CancellationToken ct = default)
    {
        var claimsPrincipal = await FetchClaimsAsync(ct);
        if (claimsPrincipal.Identity?.IsAuthenticated ?? false)
        {
            AuthenticationState = new AuthenticationState(claimsPrincipal);
        }
    }

    public async Task LoginAsync(UserLoginCommand command, CancellationToken ct = default)
    {
        var result = await _http.PostAsJsonAsync("api/v1/auth/login", command, ct);

        if (result.IsSuccessStatusCode)
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        // TODO: handle error
    }

    private async Task<ClaimsPrincipal> FetchClaimsAsync(CancellationToken ct = default)
    {
        var resp = await _http.GetAsync("api/v1/auth/user_claims", ct);
        if (!resp.IsSuccessStatusCode)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

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