using Crypton.Application.Economy;
using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.ModelBinders;
using Crypton.Infrastructure.RateLimiting;
using Crypton.WebAPI.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace Crypton.WebAPI.Controllers;

// TODO implement an abstract ApiController class
// with injected IMediator and ICurrentUserAccessor
// and HandleRequest<TRequest, TResponse> method.

/// <summary>
/// Transaction controller for handling the creation and mining of transactions.
/// </summary>
[Authorize]
public sealed class TransactionController : ApiController
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionController"/> class.
    /// </summary>
    public TransactionController(ILogger<TransactionController> logger, IMediator mediator)
        : base(logger, mediator)
    {
    }

    /// <summary>
    /// Collect daily coins.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
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

        await this.TryHandleCommandAsync<CollectDailyCommand>(command, ct);

        var result = await this.Mediator.Send(command, ct);

        if (result.IsError)
            return this.BadRequest(result.Errors);

        if (result.Value is CollectDailyResult.NotReadyYet { CollectAt: var collectAt })
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
        [FromBody, BindRequired] [ModelBinder(BinderType = typeof(CreateTransactionCommandModelBinder))]
        CreateTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await this.mediator.Send(command, ct);

        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.StatusCode(StatusCodes.Status202Accepted);
    }
}