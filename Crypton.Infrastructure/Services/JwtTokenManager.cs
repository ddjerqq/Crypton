// <copyright file="JwtTokenManager.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Crypton.Infrastructure.Services;

public static class JwtTokenManager
{
    private static readonly JwtSecurityTokenHandler Handler = new();

    private static SymmetricSecurityKey Key
    {
        get
        {
            string key = Environment.GetEnvironmentVariable("JWT__KEY")
                         ?? throw new ArgumentException("JWT__KEY is not present in the environment");

            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }

    private static SigningCredentials Creds => new(Key, SecurityAlgorithms.HmacSha256);

    private static TimeSpan Expiration
    {
        get
        {
            var expiration = Environment.GetEnvironmentVariable("JWT__EXPIRATION");

            return int.TryParse(expiration, out int minutes)
                ? TimeSpan.FromMinutes(minutes)
                : TimeSpan.FromDays(31);
        }
    }

    public static string GenerateToken(IEnumerable<Claim> claims)
    {
        var token = new JwtSecurityToken(
            Environment.GetEnvironmentVariable("JWT__ISSUER"),
            Environment.GetEnvironmentVariable("JWT__AUDIENCE"),
            claims,
            expires: DateTime.UtcNow.Add(Expiration),
            signingCredentials: Creds);

        return Handler.WriteToken(token);
    }
}