// <copyright file="ConfigureServices.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Text;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Crypton.Infrastructure.Identity;

public static class ConfigureServices
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddAuthentication()
            .AddJwtBearer(ConfigureJwtBearerOptions);

        services.AddIdentity<User, IdentityRole>(ConfigureIdentityOptions).AddEntityFrameworkStores<AppDbContext>();

        services.ConfigureApplicationCookie(ConfigureApplicationCookieOptions);

        services.Configure<AuthenticationOptions>(ConfigureAuthenticationSchemas);

        services.Configure<CookieAuthenticationOptions>(
            IdentityConstants.ApplicationScheme,
            ConfigureCookieAuthenticationOptions);

        services.AddAuthorization();

        return services;
    }

    private static void ConfigureAuthenticationSchemas(AuthenticationOptions options)
    {
        options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    }

    private static void ConfigureJwtBearerOptions(JwtBearerOptions options)
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = IdentityEvents.OnJwtBearerMessageReceived,
        };

        options.MapInboundClaims = false;

        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.ClaimsIssuer = Environment.GetEnvironmentVariable("JWT__AUDIENCE");
        options.Audience = Environment.GetEnvironmentVariable("JWT__ISSUER");

        string key = Environment.GetEnvironmentVariable("JWT__KEY")
                     ?? throw new ArgumentException("JWT__KEY is not present in the environment");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = Environment.GetEnvironmentVariable("JWT__ISSUER") is var issuer,
            ValidIssuer = issuer,

            ValidateAudience = Environment.GetEnvironmentVariable("JWT__AUDIENCE") is var audience,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey,

            ValidateLifetime = true,
        };
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.User.AllowedUserNameCharacters = ApplicationAuthConstants.AllowedUserNameCharacters;
        options.User.RequireUniqueEmail = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.AllowedForNewUsers = false;
        options.Lockout.MaxFailedAccessAttempts = 5;

        options.Password.RequiredLength = 5;
        options.Password.RequiredUniqueChars = 1;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    }

    private static void ConfigureApplicationCookieOptions(CookieAuthenticationOptions options)
    {
        options.Cookie.Name = ApplicationAuthConstants.CookieName;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true;
        options.Cookie.MaxAge = TimeSpan.FromDays(31);
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;

        options.SlidingExpiration = true;
    }

    private static void ConfigureCookieAuthenticationOptions(CookieAuthenticationOptions options)
    {
        options.Events.OnSignedIn = IdentityEvents.OnIdentitySignedIn;
        options.Events.OnRedirectToLogin = IdentityEvents.OnRedirectToLogin;
        options.Events.OnRedirectToAccessDenied = IdentityEvents.OnRedirectToAccessDenied;
    }
}