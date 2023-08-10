using Crypton.Domain.Entities;

namespace Crypton.Application.Interfaces;

public interface ICurrentUserAccessor
{
    public string? GetCurrentUserId();

    public Task<User?> GetCurrentUserAsync(CancellationToken ct = default);
}