using Crypton.Application.Economy;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.ModelBinders;
using Crypton.Infrastructure.RateLimiting;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace Crypton.WebAPI.Controllers;

[Authorize]
[ApiController]
[Produces("application/json")]
[Route("/api/v1/[controller]")]
public sealed class TransactionController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    /// <summary>
    /// Collect daily coins.
    /// </summary>
    /// <response code="201">Success</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Rate Limited, or not ready for daily</response>
    /// <response code="500">Internal Server Error</response>
    [RequireIdempotency]
    [EnableRateLimiting(RateLimitConstants.TransactionPolicyName)]
    [HttpPost("daily")]
    [ProducesResponseType(201)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CollectDaily(CancellationToken ct = default)
    {
        var command = new CollectDailyCommand();

        var result = await this._mediator.Send(command, ct);

        if (result is CollectDailyResult.NotReadyYet { CollectAt: var collectAt })
        {
            var retryAfter = collectAt.AddDays(1).ToString("R");
            this.HttpContext.Response.Headers[HeaderNames.RetryAfter] = retryAfter;
            return this.StatusCode(StatusCodes.Status429TooManyRequests, retryAfter);
        }

        return this.StatusCode(StatusCodes.Status202Accepted);
    }

    /// <summary>
    /// Create a transaction.
    /// </summary>
    /// <param name="command">The transaction creation command, either amount or itemId must be supplied.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <response code="201">Success</response>
    /// <response code="400">Bad Request</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Rate Limited, or not ready for daily</response>
    /// <response code="500">Internal Server Error</response>
    [RequireIdempotency]
    [EnableRateLimiting(RateLimitConstants.TransactionPolicyName)]
    [HttpPost("create")]
    [ProducesResponseType(201)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] [BindRequired] [ModelBinder(BinderType = typeof(CreateTransactionCommandModelBinder))]
        CreateTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await this._mediator.Send(command, ct);

        if (result is CreateTransactionResult.InsufficientFunds or CreateTransactionResult.InvalidItem)
            return this.BadRequest("Insufficient funds or invalid item.");

        return this.StatusCode(StatusCodes.Status202Accepted);
    }
}