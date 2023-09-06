using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Inventory.Commands;

public sealed record SellItemCommand(Guid ItemId) : IRequest<ErrorOr<decimal>>;

internal sealed class SellItemHandler : IRequestHandler<SellItemCommand, ErrorOr<decimal>>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public SellItemHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<ErrorOr<decimal>> Handle(SellItemCommand request, CancellationToken ct)
    {
        var sender = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.User.Unauthenticated;

        var item = await _dbContext.Set<Item>()
            .Include(x => x.ItemType)
            .FirstOrDefaultAsync(x => x.Id == request.ItemId, ct);

        // if the item does not exist, or the sender does not have it
        if (item is null || !sender.Inventory.HasItemWithId(item.Id))
            return Errors.Inventory.InvalidItem;

        var command = new CreateTransactionCommand(sender, null, item);
        var result = await _mediator.Send(command, ct);
        if (result.IsError)
            return result.Errors!;

        return item.Price;
    }
}