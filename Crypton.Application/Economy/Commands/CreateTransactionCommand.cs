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

internal sealed class CreateTransactionHandler : IRequestHandler<CreateTransactionCommand, IErrorOr>
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

        this._dbContext.Set<User>().TryUpdateIfNotNull(request.Sender);
        this._dbContext.Set<User>().TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }

    private async ValueTask<IErrorOr> HandleItemTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // if sender is not null
        if (request.Sender is not null)
        {
            // transfer items between inventories
            if (!request.Sender.Inventory.HasItemWithId(request.ItemId!.Value))
                return Errors.From(Errors.Economy.InvalidItem);

            // we assume that the receiver is never null, because items
            // should not be destroyed, only transferred between inventories
            request.Sender.Inventory.Transfer(
                request.Receiver!.Inventory,
                request.ItemId!.Value,
                request.Receiver);
        }
        else
        {
            // add the item to the receiver's inventory
            await this._dbContext.Set<Item>()
                .AddAsync(request.Item!, ct);

            request.Receiver!.Inventory.Add(request.Item!);
        }

        request.Receiver?.AddDomainEvent(new ItemReceivedEvent(request.ItemId!.Value));

        this._dbContext.Set<User>().TryUpdateIfNotNull(request.Sender);
        this._dbContext.Set<User>().TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return Errors.Success;
    }
}