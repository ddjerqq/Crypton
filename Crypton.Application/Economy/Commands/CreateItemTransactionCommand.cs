using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Economy.Commands;

public sealed record CreateItemTransactionCommand(Guid ItemId, Guid ReceiverId) : IRequest<IErrorOr>;

public sealed class CreateItemTransactionHandler : IRequestHandler<CreateItemTransactionCommand, IErrorOr>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public CreateItemTransactionHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        this._mediator = mediator;
        this._dbContext = dbContext;
        this._currentUserAccessor = currentUserAccessor;
    }

    public async Task<IErrorOr> Handle(CreateItemTransactionCommand request, CancellationToken ct)
    {
        var sender = await this._currentUserAccessor
            .GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await this._dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Id == request.ReceiverId, ct);
        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var item = sender.Inventory.FirstOrDefault(x => x.Id == request.ItemId);
        if (item is null)
            return Errors.From(Errors.Economy.InvalidItem);

        var command = new CreateTransactionCommand(sender, receiver, item);
        return await this._mediator.Send(command, ct);
    }
}