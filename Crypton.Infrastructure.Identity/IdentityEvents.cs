// <copyright file="IdentityEvents.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using MessageReceivedContext = Microsoft.AspNetCore.Authentication.JwtBearer.MessageReceivedContext;

namespace Crypton.Infrastructure.Identity;

public static class IdentityEvents
{
    // this intercepts the token in the request and sets it as the token for the JWT bearer authentication scheme
    public static Task OnJwtBearerMessageReceived(MessageReceivedContext context)
    {
        context.Request.Cookies.TryGetValue(ApplicationAuthConstants.CookieName, out var cookieToken);
        context.Request.Headers.TryGetValue(ApplicationAuthConstants.CookieName, out var headerToken);
        context.Request.Query.TryGetValue(ApplicationAuthConstants.CookieName, out var queryToken);

        context.Token = (string?)queryToken ?? (string?)headerToken ?? cookieToken;

        return Task.CompletedTask;
    }

    // overwrite the cookie with a JWT token
    public static Task OnIdentitySignedIn(CookieSignedInContext context)
    {
        if (context.Principal is not null)
        {
            string token = JwtTokenManager.GenerateToken(context.Principal.Claims);

            context.Response.Cookies.Delete(ApplicationAuthConstants.CookieName);
            context.Response.Cookies.Append(ApplicationAuthConstants.CookieName, token, new CookieOptions
            {
                SameSite = SameSiteMode.Strict,
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                MaxAge = TimeSpan.FromDays(31),
            });
        }

        return Task.CompletedTask;
    }

    // when we have api calls, we will return 401 instead of redirecting to login
    public static Task OnRedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        // todo use reflection to test for the controller having the [ApiController] attribute
        if (context.Request.Path.Value?.StartsWith("/api") ?? false)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    }

    // when we have api calls, we will return 403 instead of redirecting to access denied
    public static Task OnRedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        // todo use reflection to test for the controller having the [ApiController] attribute
        if (context.Request.Path.Value?.StartsWith("/api") ?? false)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
        else
        {
            context.Response.Redirect(context.RedirectUri);
        }

        return Task.CompletedTask;
    }
}