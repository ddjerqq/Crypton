using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Crypton.Application.Common.Extensions;
using Crypton.Application.Interfaces;
using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

public abstract record CreateTransactionResult
{
    public sealed record Success : CreateTransactionResult;

    public sealed record InsufficientFunds : CreateTransactionResult;

    public sealed record InvalidItem : CreateTransactionResult;
}

public sealed class CreateTransactionCommand : IRequest<CreateTransactionResult>
{
    // for Json Construction
    public CreateTransactionCommand()
    {
    }

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

    [JsonIgnore]
    public Guid? SenderId { get; private set; }

    [JsonIgnore]
    public User? Sender { get; private set; }

    [JsonRequired]
    [RegularExpression("^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$")]
    public Guid? ReceiverId { get; init; }

    [JsonIgnore]
    public User? Receiver { get; private set; }

    [JsonRequired]
    [JsonPropertyName("transaction_type")]
    public TransactionType TransactionType { get; init; } = TransactionType.BalanceTransaction;

    [JsonRequired]
    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("item_id")]
    [RegularExpression("^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$")]
    public Guid? ItemId { get; init; }

    [JsonIgnore]
    public Item? Item { get; private set; }

    // Json Conversion initializer
    public async ValueTask<bool> BindAsync(
        IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken ct = default)
    {
        var currentUserId = currentUserAccessor.GetCurrentUserId();

        if (currentUserId is null)
            return false;

        this.SenderId = currentUserId.Value;
        this.Sender = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == this.SenderId, ct);

        this.Receiver = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Id == this.ReceiverId, ct);

        if (this.TransactionType == TransactionType.ItemTransaction)
        {
            this.Item = await dbContext.Items
                .FirstOrDefaultAsync(x => x.Id == this.ItemId, ct);
        }

        return true;
    }
}

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        this.ClassLevelCascadeMode = CascadeMode.Stop;
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
                .GreaterThan(0)
                .WithMessage("Amount must be a positive integer.")
                .Must((ctx, amount) => ctx.Sender?.Wallet.HasBalance(amount!.Value) ?? true)
                .WithMessage("Sender has insufficient funds.");
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

public sealed class CreateTransactionCommandHandler : IRequestHandler<CreateTransactionCommand, CreateTransactionResult>
{
    private readonly IAppDbContext _dbContext;

    public CreateTransactionCommandHandler(IAppDbContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<CreateTransactionResult> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        return request.TransactionType switch
        {
            TransactionType.BalanceTransaction => await this.HandleBalanceTransaction(request, ct),
            TransactionType.ItemTransaction => await this.HandleItemTransaction(request, ct),
            _ => throw new ArgumentOutOfRangeException(nameof(request.TransactionType), "no such transaction type"),
        };
    }

    private async ValueTask<CreateTransactionResult> HandleBalanceTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // either use the sender's wallet, or create a new wallet with infinite balance
        Wallet wallet = request.Sender is { Wallet: var senderWallet }
            ? senderWallet
            : new Wallet(decimal.MaxValue);

        if (!wallet.HasBalance(request.Amount!.Value))
            return new CreateTransactionResult.InsufficientFunds();

        wallet.Transfer(request.Receiver!.Wallet, request.Amount!.Value);

        this._dbContext.Users.TryUpdateIfNotNull(request.Sender);
        this._dbContext.Users.TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return new CreateTransactionResult.Success();
    }

    private async ValueTask<CreateTransactionResult> HandleItemTransaction(
        CreateTransactionCommand request,
        CancellationToken ct)
    {
        // TODO: i dont know what to do here, so we will not do anything for now
        throw new NotImplementedException();

        Inventory inventory = request.Sender is { Inventory: var senderInventory }
            ? senderInventory
            : new Inventory();

        if (!inventory.HasItemWithId(request.ItemId!.Value))
            return new CreateTransactionResult.InvalidItem();

        inventory.Transfer(request.Receiver!.Inventory, request.ItemId!.Value);

        this._dbContext.Users.TryUpdateIfNotNull(request.Sender);
        this._dbContext.Users.TryUpdateIfNotNull(request.Receiver);
        await this._dbContext.SaveChangesAsync(ct);

        return new CreateTransactionResult.Success();
    }
}