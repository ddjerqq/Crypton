using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Inventory.Commands;

public sealed record SendItemTransactionCommand(Guid ItemId, Guid ReceiverId) : IRequest<IErrorOr>;

internal sealed class SendItemTransactionHandler : IRequestHandler<SendItemTransactionCommand, IErrorOr>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public SendItemTransactionHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<IErrorOr> Handle(SendItemTransactionCommand request, CancellationToken ct)
    {
        var sender = await _currentUserAccessor
            .GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await _dbContext.Set<User>()
            .FirstOrDefaultAsync(x => x.Id == request.ReceiverId, ct);
        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var item = await _dbContext.Set<Item>()
            .FirstOrDefaultAsync(x => x.Id == request.ItemId, ct);

        // if the item does not exist, or the sender does not have it
        if (item is null || !sender.Inventory.HasItemWithId(item.Id))
            return Errors.From(Errors.Inventory.InvalidItem);

        var command = new CreateTransactionCommand(sender, receiver, item);
        return await _mediator.Send(command, ct);
    }
}