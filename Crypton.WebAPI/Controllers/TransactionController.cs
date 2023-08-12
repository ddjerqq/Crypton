using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.ModelBinders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
[Produces("application/json")]
public sealed class TransactionController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly ITransactionService transactionService;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public TransactionController(
        IMediator mediator,
        ITransactionService transactionService,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.mediator = mediator;
        this.transactionService = transactionService;
        this.currentUserAccessor = currentUserAccessor;
    }

    [HttpGet]
    public IActionResult GetBlocks()
    {
        var orderedTransactions = this.transactionService.Transactions
            .OrderBy(x => x.Index)
            .Select(x => new
            {
                x.Id,
                x.Index,
                SenderId = x.Sender.Id,
                SenderUserName = x.Sender.UserName,
                ReceiverId = x.Receiver.Id,
                ReceiverUserName = x.Receiver.UserName,

                ItemId = (x as ItemTransaction)?.Item.Id,
                ItemType = (x as ItemTransaction)?.Item.ItemType,

                Amount = (x as BalanceTransaction)?.Amount,

                x.Timestamp,
                x.Nonce,
                x.PreviousHash,
                x.Hash,
            })
            .ToList();

        return this.Ok(orderedTransactions);
    }

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

    [HttpPost("create")]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] [ModelBinder(BinderType = typeof(CreateTransactionCommandModelBinder))]
        CreateTransactionCommand command,
        CancellationToken ct = default)
    {
        var result = await this.mediator.Send(command, ct);

        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.Ok();
    }
}