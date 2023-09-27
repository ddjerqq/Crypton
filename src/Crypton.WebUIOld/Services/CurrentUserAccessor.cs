using System.Net.Http.Json;
using System.Security.Claims;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Entities;

namespace Crypton.WebUIOld.Services;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly HttpClient _http;
    private readonly CookieAuthenticationStateProvider _auth;

    public CurrentUserAccessor(HttpClient http, CookieAuthenticationStateProvider auth)
    {
        _auth = auth;
        _http = http;
    }

    public Guid? GetCurrentUserId()
    {
        if (_auth.AuthenticationState.User.Identity?.IsAuthenticated ?? false)
        {
            // TODO: or try get it from the API
            return null;
        }

        var idClaim = _auth.AuthenticationState.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var userId) ? userId : null;
    }

    public async Task<User?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (_auth.AuthenticationState.User.Identity?.IsAuthenticated ?? false)
        {
            // TODO: or try get it from the API
            return null;
        }

        var resp = await _http.GetAsync("api/v1/auth/user", ct);
        if (!resp.IsSuccessStatusCode)
            return null;

        return await resp.Content.ReadFromJsonAsync<User>(ct);
    }
}