using Crypton.Application.Dtos;
using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using Crypton.Infrastructure.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Controllers;

/// <summary>
/// Transaction controller for handling the creation and mining of transactions.
/// </summary>
[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
[Produces("application/json")]
public sealed class TransactionController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ITransactionService transactionService;
    private readonly ICurrentUserAccessor currentUserAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionController"/> class.
    /// </summary>
    /// <param name="mediator">Mediator.</param>
    /// <param name="transactionService">TransactionService.</param>
    /// <param name="currentUserAccessor">CurrentUserAccessor.</param>
    public TransactionController(
        IMediator mediator,
        ITransactionService transactionService,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.mediator = mediator;
        this.transactionService = transactionService;
        this.currentUserAccessor = currentUserAccessor;
    }

    /// <summary>
    /// Get all transactions.
    /// </summary>
    /// <returns>a list of transaction dtos.</returns>
    [Produces<IEnumerable<TransactionDto>>]
    [HttpGet("all")]
    public IActionResult AllTransactions()
    {
        var orderedTransactions = this.transactionService.Transactions
            .OrderBy(x => x.Index)
            .Select(x => (TransactionDto)x)
            .ToList();

        return this.Ok(orderedTransactions);
    }

    /// <summary>
    /// Collect daily coins.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>status code 200 if the user is eligible for the daily reward, status code 400 otherwise.</returns>
    [HttpPost("daily")]
    public async Task<IActionResult> CollectDaily(CancellationToken ct = default)
    {
        var currentUser = await this.currentUserAccessor.GetCurrentUserAsync(ct);

        if (currentUser is null)
            return this.Unauthorized();

        var command = new CreateTransactionCommand(null, currentUser, 100);

        var result = await this.mediator.Send(command, ct);
        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.Ok();
    }

    /// <summary>
    /// Create a transaction.
    /// </summary>
    /// <param name="command">The transaction creation command, either amount or itemId must be supplied.</param>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>status code 202 if the command if valid, status code 400 otherwise, along ValidationErrors.</returns>
    [HttpPost("create")]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] [ModelBinder(BinderType = typeof(CreateTransactionCommandModelBinder))]
        CreateTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await this.mediator.Send(command, ct);

        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.StatusCode(StatusCodes.Status202Accepted);
    }
}