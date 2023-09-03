using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;

namespace Crypton.Application.Economy;

public sealed class CollectDailyCommand : IRequest<ErrorOr<decimal>>
{
}

public sealed class CollectDailyHandler : IRequestHandler<CollectDailyCommand, ErrorOr<decimal>>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CollectDailyHandler(
        IMediator mediator,
        IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        this._mediator = mediator;
        this._dbContext = dbContext;
        this._currentUserAccessor = currentUserAccessor;
    }

    public async Task<ErrorOr<decimal>> Handle(CollectDailyCommand request, CancellationToken ct)
    {
        var currentUser = await this._currentUserAccessor.GetCurrentUserAsync(ct);
        if (currentUser is null)
            return Errors.User.Unauthenticated;

        // daily already collected
        if (!currentUser.DailyStreak.IsEligibleForDaily())
            return Errors.User.DailyNotReady;

        currentUser.DailyStreak.CollectDaily();
        this._dbContext.Set<User>().Update(currentUser);
        await this._dbContext.SaveChangesAsync(ct);

        var amount = currentUser.DailyStreak.Streak * 100;

        var command = new CreateTransactionCommand(null, currentUser, amount);
        await this._mediator.Send(command, ct);

        return amount;
    }
}