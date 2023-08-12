using System.Security.Claims;
using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Infrastructure.Services;

public sealed class CurrentUserAccessor : ICurrentUserAccessor
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<User> userManager;

    public CurrentUserAccessor(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
    }

    public Guid? GetCurrentUserId()
    {
        var claimsPrincipal = this.httpContextAccessor.HttpContext?.User;
        var stringId = claimsPrincipal?
            .Claims
            .FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?
            .Value;

        return Guid.TryParse(stringId, out var id) ? id : null;
    }

    public async Task<User?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var claimsPrincipal = this.httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal is null) return null;

        return await this.userManager.GetUserAsync(claimsPrincipal);
    }
}