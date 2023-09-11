namespace Crypton.Infrastructure.Services.Interfaces;

/// <summary>
/// cache of keys to check for idempotency
/// </summary>
public interface IIdempotencyService
{
    public bool ContainsKey(Guid key);

    public void AddKey(Guid key);
}