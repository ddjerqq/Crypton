using Crypton.Domain.Entities;

namespace Crypton.Application.Common.Interfaces;

public interface ICurrentUserAccessor
{
    public Guid? GetCurrentUserId();

    public Task<User?> GetCurrentUserAsync(CancellationToken ct = default);
}