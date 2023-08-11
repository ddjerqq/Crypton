using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
[Produces("application/json")]
public sealed class TransactionController : ControllerBase
{
    private readonly IMediator mediator;
    private readonly IAppDbContext dbContext;
    private readonly ITransactionService transactionService;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public TransactionController(
        IMediator mediator,
        IAppDbContext dbContext,
        ITransactionService transactionService,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.mediator = mediator;
        this.dbContext = dbContext;
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

    [HttpGet("{id:guid}")]
    public IActionResult GetBlockById(Guid id)
    {
        var block = this.transactionService.Transactions
            .FirstOrDefault(x => x.Id == id);

        if (block is null)
            return this.NotFound();

        return this.Ok(block);
    }

    [HttpGet("{id:long}")]
    public IActionResult GetBlockByIndex(long id)
    {
        var block = this.transactionService.Transactions
            .FirstOrDefault(x => x.Index == id);

        if (block is null)
            return this.NotFound();

        return this.Ok(block);
    }

    [HttpPost("daily")]
    public async Task<IActionResult> CollectDaily(CancellationToken ct = default)
    {
        var currentUser = await this.currentUserAccessor.GetCurrentUserAsync(ct);

        var command = new CreateTransactionCommand
        {
            SenderId = GuidExtensions.ZeroGuidValue,
            Sender = Domain.Entities.User.SystemUser(),
            ReceiverId = currentUser!.Id,
            Receiver = currentUser,
            TransactionType = TransactionType.BalanceTransaction,
            Amount = 100,
            ItemId = null,
            Item = null,
        };

        var result = await this.mediator.Send(command, ct);
        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.Ok();
    }

    // TODO this should be a validatable command
    [HttpPost("send_payment/{receiverId:guid}")]
    public async Task<IActionResult> CreateTransaction(
        Guid receiverId,
        [FromQuery] decimal amount,
        CancellationToken ct = default)
    {
        var currentUser = await this.currentUserAccessor.GetCurrentUserAsync(ct);
        var receiver = await this.dbContext
            .Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == receiverId.ToString(), ct);

        // these will need to go to MediatR
        if (currentUser is null || receiver is null)
            return this.BadRequest("Invalid user id");

        if (amount <= 0 || amount > currentUser.Balance)
            return this.BadRequest("Invalid amount");

        var command = new CreateTransactionCommand
        {
            SenderId = currentUser.Id,
            Sender = currentUser,
            ReceiverId = receiver.Id,
            Receiver = receiver,
            TransactionType = TransactionType.BalanceTransaction,
            Amount = amount,
            ItemId = null,
            Item = null,
        };

        var result = await this.mediator.Send(command, ct);
        if (result.IsError)
            return this.BadRequest(result.Errors);

        return this.Ok();
    }
}