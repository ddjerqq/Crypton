using Crypton.Application.Economy;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.RateLimiting;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Crypton.WebAPI.Controllers;

public sealed class EconomyController : ApiController
{
    /// <summary>
    /// Collect daily coins.
    /// </summary>
    [RequireIdempotency]
    [Cooldown(1, 86400)]
    [HttpPost("daily")]
    public async Task<IActionResult> CollectDaily(CancellationToken ct)
    {
        var command = new CollectDailyCommand();
        var collected = await this.HandleCommandAsync<CollectDailyCommand, decimal>(command, ct);
        return this.Ok(collected);
    }

    /// <summary>
    /// Create a balance transaction.
    /// </summary>
    [RequireIdempotency]
    [Cooldown(1, 10)]
    [HttpPost("create_balance")]
    public async Task<IActionResult> CreateBalanceTransaction(CreateBalanceTransactionCommand command, CancellationToken ct)
    {
        await this.HandleCommandAsync(command, ct);
        return this.Ok();
    }
}