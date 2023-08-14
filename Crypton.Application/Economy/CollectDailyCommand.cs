using Crypton.Application.Common.Abstractions;
using Crypton.Application.Interfaces;
using Crypton.Application.Transactions;
using ErrorOr;
using MediatR;

namespace Crypton.Application.Economy;

public abstract record CollectDailyResult
{
    public sealed record Success : CollectDailyResult;

    public sealed record NotReadyYet(DateTime CollectAt) : CollectDailyResult;
}

public sealed class CollectDailyCommand : IResultRequest<CollectDailyResult>
{
}

public sealed class CollectDailyCommandHandler : IResultRequestHandler<CollectDailyCommand, CollectDailyResult>
{
    private readonly IMediator mediator;
    private readonly IAppDbContext dbContext;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public CollectDailyCommandHandler(IMediator mediator, IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.mediator = mediator;
        this.dbContext = dbContext;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<ErrorOr<CollectDailyResult>> Handle(CollectDailyCommand request, CancellationToken ct)
    {
        var currentUser = await this.currentUserAccessor.GetCurrentUserAsync(ct);

        if (currentUser is null)
            return Error.Failure("unauthorized");

        // daily collected
        if (!currentUser.IsEligibleForDaily())
        {
            return new CollectDailyResult.NotReadyYet(currentUser.DailyCollectedAt.AddDays(1));
        }

        currentUser.CollectDaily();
        this.dbContext.Users.Update(currentUser);
        await this.dbContext.SaveChangesAsync(ct);

        var amount = currentUser.DailyStreak * 100;
        var command = new CreateTransactionCommand(null, currentUser, amount);

        var result = await this.mediator.Send(command, ct);
        if (result.IsError || result.Value is false)
            return result.Errors;

        return new CollectDailyResult.Success();
    }
}