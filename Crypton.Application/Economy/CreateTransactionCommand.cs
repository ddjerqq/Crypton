using Crypton.Application.Common.Extensions;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Economy;

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

public sealed class CreateTransactionCommand : IRequest<IErrorOr>
{
    public CreateTransactionCommand(User? sender, User? receiver, decimal amount)
    {
        this.SenderId = sender?.Id;
        this.Sender = sender;

        this.ReceiverId = receiver?.Id;
        this.Receiver = receiver;

        this.TransactionType = TransactionType.BalanceTransaction;
        this.Amount = amount;
    }

    public CreateTransactionCommand(User? sender, User? receiver, Item item)
    {
        this.SenderId = sender?.Id;
        this.Sender = sender;

        this.ReceiverId = receiver?.Id;
        this.Receiver = receiver;

        this.TransactionType = TransactionType.ItemTransaction;
        this.ItemId = item.Id;
        this.Item = item;
    }

    public Guid? SenderId { get; init; }

    public User? Sender { get; init; }

    public Guid? ReceiverId { get; init; }

    public User? Receiver { get; init; }

    public TransactionType TransactionType { get; init; }

    public decimal? Amount { get; init; }

    public Guid? ItemId { get; init; }

    public Item? Item { get; init; }
}

public sealed class CreateTransactionValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionValidator()
    {
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.SenderId)
            .NotEqual(x => x.ReceiverId)
            .WithMessage("Sender and Receiver are the same user.")
            .When(x => x.SenderId.HasValue);

        this.RuleFor(x => x.Receiver)
            .NotEmpty();

        this.RuleFor(x => x.ReceiverId)
            .NotEmpty()
            .NotEqual(x => x.SenderId)
            .WithMessage("Sender and Receiver are the same user.");

        this.When(x => x.TransactionType == TransactionType.BalanceTransaction, () =>
        {
            this.RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0);
        });

        this.When(x => x.TransactionType == TransactionType.ItemTransaction, () =>
        {
            this.RuleFor(x => x.ItemId)
                .NotEmpty()
                .Must((ctx, itemId) => ctx.Sender?.Inventory.HasItemWithId(itemId!.Value) ?? true)
                .WithMessage("Sender does not own the item.");

            this.RuleFor(x => x.Item)
                .NotEmpty();
        });
    }
}

public sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, IErrorOr>
{
    private readonly IAppDbContext _dbContext;

    public CreateTransactionHandler(IAppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<IErrorOr> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        return request.TransactionType switch
        {
            TransactionType.BalanceTransaction => await this.HandleBalanceTransaction(request, ct),
            TransactionType.ItemTransaction => await this.HandleItemTransaction(request, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request.TransactionType), "no such transaction type"),
        };
    }

    private async ValueTask<IErrorOr> HandleBalanceTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // either use the sender's wallet, or create a new wallet with infinite balance
        Wallet wallet = request.Sender is { Wallet: var senderWallet }
            ? senderWallet
            : new Wallet(decimal.MaxValue);

        if (!wallet.HasBalance(request.Amount!.Value))
            return Errors.From(Errors.Economy.InsufficientFunds);

        wallet.Transfer(request.Receiver!.Wallet, request.Amount!.Value);

        this._dbContext.Users.TryUpdateIfNotNull(request.Sender);
        this._dbContext.Users.TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    private async ValueTask<IErrorOr> HandleItemTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // TODO: i dont know what to do here, so we will not do anything for now
        throw new NotImplementedException();

        Domain.ValueObjects.Inventory inventory = request.Sender is { Inventory: var senderInventory }
            ? senderInventory
            : new Domain.ValueObjects.Inventory();

        if (!inventory.HasItemWithId(request.ItemId!.Value))
            return Errors.From(Errors.Economy.InvalidItem);

        inventory.Transfer(request.Receiver!.Inventory, request.ItemId!.Value);

        this._dbContext.Users.TryUpdateIfNotNull(request.Sender);
        this._dbContext.Users.TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }
}