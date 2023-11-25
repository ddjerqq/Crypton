using Crypton.Application.Common.Extensions;
using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Economy.Handlers;

internal sealed class BalanceTransactionHandler
    : IRequestHandler<BalanceTransactionCommand, IErrorOr>,
        IRequestHandler<SendBalanceCommand, IErrorOr>,
        IRequestHandler<CollectDailyCommand, ErrorOr<decimal>>
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public BalanceTransactionHandler(IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    // base balance transaction
    public async Task<IErrorOr> Handle(BalanceTransactionCommand command, CancellationToken ct)
    {
        // TODO: we will have a balance / transaction validation service here
        // if (!command.SenderWallet.HasBalance(command.Amount))
        //     return Errors.From(Errors.Economy.InsufficientFunds);
        //
        // command.SenderWallet.Transfer(command.ReceiverWallet, command.Amount);

        _dbContext.Set<User>().TryUpdateIfNotNull(command.Sender);
        _dbContext.Set<User>().TryUpdateIfNotNull(command.Receiver);
        await _dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    // sender: current | receiver: other
    public async Task<IErrorOr> Handle(SendBalanceCommand command, CancellationToken ct)
    {
        var sender = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Id == command.ReceiverId, ct);
        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var transactionCommand = new BalanceTransactionCommand(sender, receiver, command.Amount);
        return await Handle(transactionCommand, ct);
    }

    // sender: null | receiver: current
    public async Task<ErrorOr<decimal>> Handle(CollectDailyCommand command, CancellationToken ct)
    {
        var receiver = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (receiver is null)
            return Errors.User.Unauthenticated;

        // daily already collected
        if (!receiver.DailyStreak.IsEligibleForDaily())
            return Errors.User.DailyNotReady(receiver.DailyStreak.CollectNextDailyAfter);

        receiver.DailyStreak.CollectDaily();

        // _dbContext.Set<User>().Update(receiver);
        // await _dbContext.SaveChangesAsync(ct);

        var amount = receiver.DailyStreak.Streak * 100;

        var transactionCommand = new BalanceTransactionCommand(null, receiver, amount);
        var transactionResult = await Handle(transactionCommand, ct);
        if (transactionResult.IsError)
            return transactionResult.Errors!;

        return amount;
    }
}