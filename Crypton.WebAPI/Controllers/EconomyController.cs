using Crypton.Application.Economy.Commands;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.RateLimiting;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Crypton.WebAPI.Controllers;

[Authorize]
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
        var collected = await HandleCommandAsync<CollectDailyCommand, decimal>(command, ct);
        return Ok(collected);
    }

    /// <summary>
    /// Create a balance transaction.
    /// </summary>
    [RequireIdempotency]
    [Cooldown(1, 5)]
    [HttpPost("create_balance")]
    public async Task<IActionResult> CreateBalanceTransaction(
        [FromBody, BindRequired] CreateBalanceTransactionCommand command,
        CancellationToken ct)
    {
        await HandleCommandAsync(command, ct);
        return Ok();
    }
}