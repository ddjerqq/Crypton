using Crypton.Application.Interfaces;
using MediatR;

namespace Crypton.Application.Economy;

public abstract record CollectDailyResult
{
    public sealed record Success(decimal AmountCollected) : CollectDailyResult;

    public sealed record NotReadyYet(DateTime CollectAt) : CollectDailyResult;
}

public sealed class CollectDailyCommand : IRequest<CollectDailyResult>
{
}

public sealed class CollectDailyCommandHandler : IRequestHandler<CollectDailyCommand, CollectDailyResult>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CollectDailyCommandHandler(
        IMediator mediator,
        IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        this._mediator = mediator;
        this._dbContext = dbContext;
        this._currentUserAccessor = currentUserAccessor;
    }

    public async Task<CollectDailyResult> Handle(CollectDailyCommand request, CancellationToken ct)
    {
        var currentUser = await this._currentUserAccessor.GetCurrentUserAsync(ct);

        // daily collected
        if (!(currentUser?.DailyStreak.IsEligibleForDaily() ?? false))
        {
            return new CollectDailyResult.NotReadyYet(currentUser?.DailyStreak.CollectNextDailyAt ?? DateTime.MaxValue);
        }

        currentUser.DailyStreak.CollectDaily();
        this._dbContext.Users.Update(currentUser);
        await this._dbContext.SaveChangesAsync(ct);

        var amount = currentUser.DailyStreak.Streak * 100;

        var command = new CreateTransactionCommand(null, currentUser, amount);
        await this._mediator.Send(command, ct);

        return new CollectDailyResult.Success(amount);
    }
}