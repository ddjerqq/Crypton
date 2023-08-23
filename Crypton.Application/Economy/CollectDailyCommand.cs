using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using MediatR;

namespace Crypton.Application.Economy;

public abstract record CollectDailyResult
{
    public sealed record Success(decimal AmountCollected) : CollectDailyResult;

    public sealed record Unauthenticated : CollectDailyResult;

    public sealed record NotReadyYet(DateTime CollectAt) : CollectDailyResult;
}

public sealed class CollectDailyCommand : IRequest<CollectDailyResult>
{
}

public sealed class CollectDailyCommandHandler : IRequestHandler<CollectDailyCommand, CollectDailyResult>
{
    private readonly IMediator mediator;
    private readonly IAppDbContext dbContext;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public CollectDailyCommandHandler(
        IMediator mediator,
        IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.mediator = mediator;
        this.dbContext = dbContext;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<CollectDailyResult> Handle(CollectDailyCommand request, CancellationToken ct)
    {
        var currentUser = await this.currentUserAccessor.GetCurrentUserAsync(ct);

        if (currentUser is null) return new CollectDailyResult.Unauthenticated();

        // daily collected
        if (!currentUser.DailyStreak.IsEligibleForDaily())
        {
            return new CollectDailyResult.NotReadyYet(currentUser.DailyStreak.CollectNextDailyAt);
        }

        currentUser.DailyStreak.CollectDaily();
        this.dbContext.Users.Update(currentUser);
        await this.dbContext.SaveChangesAsync(ct);

        var amount = currentUser.DailyStreak.Streak * 100;

        // TODO transaction here
        var command = new CreateTransactionCommand(null, currentUser, amount);

        return new CollectDailyResult.Success(amount);
    }
}