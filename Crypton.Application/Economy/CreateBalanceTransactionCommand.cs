using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Economy;

public sealed class CreateBalanceTransactionCommand : IRequest<IErrorOr>
{
    public Guid ReceiverId { get; init; }

    public decimal Amount { get; init; }
}

public sealed class CreateBalanceTransactionValidator : AbstractValidator<CreateBalanceTransactionCommand>
{
    public CreateBalanceTransactionValidator()
    {
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.ReceiverId)
            .NotEmpty();

        this.RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}

public sealed class CreateBalanceTransactionHandler : IRequestHandler<CreateBalanceTransactionCommand, IErrorOr>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateBalanceTransactionHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        this._mediator = mediator;
        this._dbContext = dbContext;
        this._currentUserAccessor = currentUserAccessor;
    }

    public async Task<IErrorOr> Handle(CreateBalanceTransactionCommand request, CancellationToken ct)
    {
        var sender = await this._currentUserAccessor.GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await this._dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Id == request.ReceiverId, ct);

        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var createTransactionCommand = new CreateTransactionCommand(sender, receiver, request.Amount);
        return await this._mediator.Send(createTransactionCommand, ct);
    }
}