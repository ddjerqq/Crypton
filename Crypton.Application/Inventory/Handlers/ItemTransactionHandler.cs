using Crypton.Application.Common.Extensions;
using Crypton.Application.Common.Interfaces;
using Crypton.Application.Economy.Commands;
using Crypton.Application.Inventory.Commands;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.Events;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Inventory.Handlers;

public sealed class ItemTransactionHandler
    : IRequestHandler<ItemTransactionCommand, IErrorOr>,
        IRequestHandler<SendItemCommand, IErrorOr>,
        IRequestHandler<BuyItemCommand, ErrorOr<Item>>,
        IRequestHandler<SellItemCommand, ErrorOr<decimal>>
{
    private readonly IMediator _mediator;
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public ItemTransactionHandler(IMediator mediator, IAppDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
    }

    // base balance transaction
    public async Task<IErrorOr> Handle(ItemTransactionCommand command, CancellationToken ct)
    {
        if (!command.SenderInventory.HasItemWithId(command.Item.Id))
            return Errors.From(Errors.Inventory.InvalidItem);

        command.SenderInventory.Transfer(command.ReceiverInventory, command.Item);

        command.Item.Owner = command.Receiver!;
        command.Item.OwnerId = command.Receiver?.Id ?? Guid.Empty;

        _dbContext.Set<User>().TryUpdateIfNotNull(command.Sender);
        _dbContext.Set<User>().TryUpdateIfNotNull(command.Receiver);

        command.Receiver?.AddDomainEvent(new ItemReceivedEvent(command.Item.Id));

        await _dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    // sender: current | receiver: other
    public async Task<IErrorOr> Handle(SendItemCommand command, CancellationToken ct)
    {
        // we use EFCore instead of UserManager here because we need to include the inventory
        var sender = await _dbContext.Set<User>()
            .Include(x => x.Inventory)
            .FirstOrDefaultAsync(x => x.Id == _currentUserAccessor.GetCurrentUserId(), ct);
        if (sender is null)
            return Errors.From(Errors.User.Unauthenticated);

        var receiver = await _dbContext.Set<User>()
            .Include(x => x.Inventory)
            .FirstOrDefaultAsync(x => x.Id == command.ReceiverId, ct);
        if (receiver is null)
            return Errors.From(Errors.User.NotFound);

        var item = await _dbContext.Set<Item>()
            .FirstOrDefaultAsync(x => x.Id == command.ItemId, ct);
        if (item is null)
            return Errors.From(Errors.Inventory.InvalidItem);

        var transactionCommand = new ItemTransactionCommand(sender, receiver, item);
        return await Handle(transactionCommand, ct);
    }

    // sender: null | receiver: current
    public async Task<ErrorOr<Item>> Handle(BuyItemCommand command, CancellationToken ct)
    {
        var currentUser = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (currentUser is null)
            return Errors.User.Unauthenticated;

        var itemType = await _dbContext.Set<ItemType>()
            .FirstOrDefaultAsync(x => x.Id == command.ItemTypeId, ct);
        if (itemType is null)
            return Errors.Inventory.InvalidItem;

        // pay for the item
        /* PAYMENT */
        var paymentCommand = new BalanceTransactionCommand(currentUser, null, itemType.Price);
        var paymentResult = await _mediator.Send(paymentCommand, ct);
        if (paymentResult.IsError)
            return paymentResult.Errors!;

        var item = itemType.CreateItem(currentUser);

        // add it to the database before invoking the transaction
        // because if we dont, saving changes will fail

        await _dbContext.Set<Item>()
            .AddAsync(item, ct);

        var itemTransaction = new ItemTransactionCommand(null, currentUser, item);
        var itemTransactionResult = await Handle(itemTransaction, ct);
        if (itemTransactionResult.IsError)
            return itemTransactionResult.Errors!;

        // add item to user's inventory
        currentUser.AddDomainEvent(new ItemReceivedEvent(item.Id));

        await _dbContext.SaveChangesAsync(ct);

        return item;
    }

    // sender: current | receiver: null
    public async Task<ErrorOr<decimal>> Handle(SellItemCommand command, CancellationToken ct)
    {
        var sender = await _currentUserAccessor.GetCurrentUserAsync(ct);
        if (sender is null)
            return Errors.User.Unauthenticated;

        var item = await _dbContext.Set<Item>()
            .Include(x => x.ItemType)
            .FirstOrDefaultAsync(x => x.Id == command.ItemId, ct);

        // if the item does not exist, or the sender does not have it
        if (item is null || !sender.Inventory.HasItemWithId(item.Id))
            return Errors.Inventory.InvalidItem;

        // remove the item from the sender's inventory
        sender.Inventory.Remove(item);

        var transactionCommand = new BalanceTransactionCommand(null, sender, item.Price);
        var transactionResult = await _mediator.Send(transactionCommand, ct);
        if (transactionResult.IsError)
            return transactionResult.Errors!;

        sender.AddDomainEvent(new ItemSoldEvent(item));

        return item.Price;
    }
}