using Crypton.Application.Common.Extensions;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.Events;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Economy.Commands;

public enum TransactionType
{
    /// <summary>
    /// A transaction that transfers balance from one user to another.
    /// </summary>
    BalanceTransaction,

    /// <summary>
    /// A transaction that transfers an item from one user to another.
    /// </summary>
    ItemTransaction,
}

internal sealed record CreateTransactionCommand : IRequest<IErrorOr>
{
    public CreateTransactionCommand(User? sender, User? receiver, decimal amount)
    {
        SenderId = sender?.Id;
        Sender = sender;

        ReceiverId = receiver?.Id;
        Receiver = receiver;

        TransactionType = TransactionType.BalanceTransaction;
        Amount = amount;
    }

    public CreateTransactionCommand(User? sender, User? receiver, Item item)
    {
        SenderId = sender?.Id;
        Sender = sender;

        ReceiverId = receiver?.Id;
        Receiver = receiver;

        TransactionType = TransactionType.ItemTransaction;
        ItemId = item.Id;
        Item = item;
    }

    public Guid? SenderId { get; }

    public User? Sender { get; }

    public Guid? ReceiverId { get; }

    public User? Receiver { get; }

    public TransactionType TransactionType { get; }

    public decimal? Amount { get; }

    public Guid? ItemId { get; }

    public Item? Item { get; }
}

internal sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SenderId)
            .NotEqual(x => x.ReceiverId)
            .WithMessage("Sender and Receiver are the same user.")
            .When(x => x.SenderId.HasValue);

        RuleFor(x => x.Receiver)
            .NotEmpty();

        RuleFor(x => x.ReceiverId)
            .NotEmpty()
            .NotEqual(x => x.SenderId)
            .WithMessage("Sender and Receiver are the same user.");

        When(x => x.TransactionType == TransactionType.BalanceTransaction, () =>
        {
            RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0);
        });

        When(x => x.TransactionType == TransactionType.ItemTransaction, () =>
        {
            RuleFor(x => x.ItemId)
                .NotEmpty()
                .Must((ctx, itemId) => ctx.Sender?.Inventory.HasItemWithId(itemId!.Value) ?? true)
                .WithMessage("Sender does not own the item.");

            RuleFor(x => x.Item)
                .NotEmpty();
        });
    }
}

internal sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, IErrorOr>
{
    private readonly IAppDbContext _dbContext;

    public CreateTransactionHandler(IAppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IErrorOr> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        if (request.TransactionType == TransactionType.BalanceTransaction)
            return await HandleBalanceTransaction(request, ct);

        IErrorOr itemTransactionResult;
        if (request.Sender is null)
            itemTransactionResult = await HandleItemCreation(request, ct);
        else if (request.Receiver is null)
            itemTransactionResult = await HandleItemSell(request, ct);
        else
            itemTransactionResult = await HandleItemTransfer(request, ct);

        if (itemTransactionResult.IsError)
            return Errors.From(itemTransactionResult.Errors!);

        _dbContext.Set<User>().TryUpdateIfNotNull(request.Sender);
        _dbContext.Set<User>().TryUpdateIfNotNull(request.Receiver);

        await _dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    private async ValueTask<IErrorOr> HandleBalanceTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // either use the sender's wallet, or create a new wallet with infinite balance
        Wallet senderWallet = request.Sender is { Wallet: var sWallet }
            ? sWallet
            : new Wallet(decimal.MaxValue);

        // either use the receiver's wallet, or create a new wallet with infinite balance
        Wallet receiverWallet = request.Receiver is { Wallet: var rWallet }
            ? rWallet
            : new Wallet();

        if (!senderWallet.HasBalance(request.Amount!.Value))
            return Errors.From(Errors.Economy.InsufficientFunds);

        senderWallet.Transfer(receiverWallet, request.Amount!.Value);

        _dbContext.Set<User>().TryUpdateIfNotNull(request.Sender);
        _dbContext.Set<User>().TryUpdateIfNotNull(request.Receiver);
        await _dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    // sender and receiver are both not null
    private ValueTask<IErrorOr> HandleItemTransfer(CreateTransactionCommand request, CancellationToken ct)
    {
        if (!request.Sender!.Inventory.HasItemWithId(request.ItemId!.Value))
            return ValueTask.FromResult(Errors.From(Errors.Inventory.InvalidItem));

        request.Sender.Inventory.Transfer(request.Receiver!.Inventory, request.ItemId!.Value, request.Receiver);
        request.Receiver!.AddDomainEvent(new ItemReceivedEvent(request.ItemId!.Value));

        return ValueTask.FromResult(Errors.Success);
    }

    // sender is null
    private async ValueTask<IErrorOr> HandleItemCreation(CreateTransactionCommand request, CancellationToken ct)
    {
        // create the item in the database
        await _dbContext.Set<Item>()
            .AddAsync(request.Item!, ct);

        // add it to the receiver's inventory
        request.Receiver!.Inventory.Add(request.Item!);
        request.Receiver!.AddDomainEvent(new ItemReceivedEvent(request.ItemId!.Value));

        return Errors.Success;
    }

    // receiver is null
    private async ValueTask<IErrorOr> HandleItemSell(CreateTransactionCommand request, CancellationToken ct)
    {
        if (!request.Sender!.Inventory.HasItemWithId(request.ItemId!.Value))
            return Errors.From(Errors.Inventory.InvalidItem);

        // remove the item from the sender's inventory
        var senderItem = request.Sender.Inventory.First(x => x.Id == request.ItemId!.Value);
        request.Sender.Inventory.Remove(senderItem);

        // give funds to the sender
        var transactionCommand = new CreateTransactionCommand(null, request.Sender, senderItem.Price);
        var transactionResult = await HandleBalanceTransaction(transactionCommand, ct);
        if (transactionResult.IsError)
            return Errors.From(transactionResult.Errors!);

        // to avoid JsonException: A possible object cycle was detected which is not supported.
        // we will set the owner to null
        senderItem.Owner = null!;
        request.Sender.AddDomainEvent(new ItemSoldEvent(senderItem));

        return Errors.Success;
    }
}