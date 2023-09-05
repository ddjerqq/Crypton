using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Economy.Commands;

public sealed record CreateBalanceTransactionCommand(Guid ReceiverId, decimal Amount) : IRequest<IErrorOr>;

public sealed class CreateBalanceTransactionValidator : AbstractValidator<CreateBalanceTransactionCommand>
{
    public CreateBalanceTransactionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ReceiverId)
            .NotEmpty();

        RuleFor(x => x.Amount)
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
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IErrorOr> Handle(CreateBalanceTransactionCommand request, CancellationToken ct)
    {
        var sender = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Id == request.ReceiverId, ct);

        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var createTransactionCommand = new CreateTransactionCommand(sender, receiver, request.Amount);
        return await _mediator.Send(createTransactionCommand, ct);
    }
}