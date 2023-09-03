using Crypton.Application.Economy;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.RateLimiting;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Crypton.WebAPI.Controllers;

public sealed class TransactionController : ApiController
{
    /// <summary>
    /// Collect daily coins.
    /// </summary>
    [RequireIdempotency]
    [EnableRateLimiting(RateLimitConstants.TransactionPolicyName)]
    [HttpPost("daily")]
    public async Task<IActionResult> CollectDaily(CancellationToken ct = default)
    {
        var command = new CollectDailyCommand();
        var collected = await this.HandleCommandAsync<CollectDailyCommand, decimal>(command, ct);
        return this.Ok(collected);
    }

    // /// <summary>
    // /// Create a transaction.
    // /// </summary>
    // [RequireIdempotency]
    // [EnableRateLimiting(RateLimitConstants.TransactionPolicyName)]
    // [HttpPost("create")]
    // public async Task<IActionResult> CreateTransaction(CreateTransactionCommand command, CancellationToken ct = default)
    // {
    //     await this.HandleCommandAsync(command, ct);
    //     return this.Ok();
    // }
}