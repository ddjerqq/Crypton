using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Inventory.Commands;

public sealed record BuyItemCommand(string ItemTypeId) : IRequest<ErrorOr<Item>>;

internal sealed class BuyItemHandler : IRequestHandler<BuyItemCommand, ErrorOr<Item>>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public BuyItemHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    public async Task<ErrorOr<Item>> Handle(BuyItemCommand request, CancellationToken ct)
    {
        var currentUser = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (currentUser is null)
            return Errors.User.Unauthenticated;

        var itemType = await _dbContext.Set<ItemType>()
            .FirstOrDefaultAsync(x => x.Id == request.ItemTypeId, ct);
        if (itemType is null)
            return Errors.Inventory.InvalidItem;

        // pay for the item
        var payCommand = new CreateTransactionCommand(currentUser, null, itemType!.Price);
        var payResult = await _mediator.Send(payCommand, ct);
        if (payResult.IsError)
            return payResult.Errors!;

        // create the item and add it to the user's inventory
        var item = itemType.CreateItem(currentUser);
        var acquireItemCommand = new CreateTransactionCommand(null, currentUser, item);
        var acquireItemCommandResult = await _mediator.Send(acquireItemCommand, ct);
        if (acquireItemCommandResult.IsError)
            return acquireItemCommandResult.Errors!;

        return item;
    }
}