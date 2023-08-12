using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Crypton.Application.Common.Abstractions;
using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using ErrorOr;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Crypton.Application.Transactions;

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

public sealed class CreateTransactionCommand : IResultRequest<bool>
{
    // for Json Construction
    public CreateTransactionCommand()
    {
    }

    public CreateTransactionCommand(User? sender, User? receiver, decimal amount)
    {
        sender ??= User.SystemUser();
        receiver ??= User.SystemUser();

        this.SenderId = sender.Id;
        this.Sender = sender;

        this.ReceiverId = receiver.Id;
        this.Receiver = receiver;

        this.TransactionType = TransactionType.BalanceTransaction;
        this.Amount = amount;
    }

    public CreateTransactionCommand(User? sender, User? receiver, Item item)
    {
        sender ??= User.SystemUser();
        receiver ??= User.SystemUser();

        this.SenderId = sender.Id;
        this.Sender = sender;

        this.ReceiverId = receiver.Id;
        this.Receiver = receiver;

        this.TransactionType = TransactionType.ItemTransaction;
        this.ItemId = item.Id;
        this.Item = item;
    }

    [JsonIgnore]
    public string SenderId { get; private set; } = string.Empty;

    [JsonIgnore]
    public User? Sender { get; private set; }

    [JsonIgnore]
    public bool IsSenderSystem => this.Sender?.IsSystem ?? this.SenderId == GuidExtensions.ZeroGuidValue;

    [JsonRequired]
    [JsonPropertyName("receiver_id")]
    [RegularExpression("^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$")]
    public string ReceiverId { get; init; } = string.Empty;

    [JsonIgnore]
    public User? Receiver { get; private set; }

    [JsonIgnore]
    public bool IsReceiverSystem => this.Receiver?.IsSystem ?? this.ReceiverId == GuidExtensions.ZeroGuidValue;

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
    public async Task<bool> InitializeAsync(
        IAppDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken ct = default)
    {
        if (!this.IsSenderSystem && (string.IsNullOrEmpty(this.SenderId) || this.Sender is null))
        {
            var currentUserId = currentUserAccessor.GetCurrentUserId();

            if (string.IsNullOrEmpty(currentUserId))
                return false;

            this.SenderId = currentUserId;
            this.Sender = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == this.SenderId, ct);
        }

        if (!this.IsReceiverSystem)
        {
            this.Receiver = await dbContext.Users.AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == this.ReceiverId, ct);
        }

        if (this.TransactionType == TransactionType.ItemTransaction)
        {
            this.Item = await dbContext.Items.AsNoTracking()
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

        this.RuleFor(x => x.Receiver)
            .NotEmpty()
            .When(x => !x.IsReceiverSystem);

        this.RuleFor(x => x.ReceiverId)
            .NotEmpty()
            .When(x => !x.IsReceiverSystem)
            .NotEqual(x => x.SenderId)
            .WithMessage("You cannot send transactions to yourself.");

        this.When(x => x.TransactionType == TransactionType.BalanceTransaction, () =>
        {
            this.RuleFor(x => x.Amount)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");

            // when the sender is not system, check if the sender has enough balance
            this.When(x => !(x.Sender?.IsSystem ?? x.SenderId == GuidExtensions.ZeroGuidValue), () =>
            {
                this.RuleFor(x => x.Amount)
                    .GreaterThan(0)
                    .WithMessage("Amount must be a positive integer.")
                    .Must((ctx, amount) => amount <= ctx.Sender!.Balance)
                    .WithMessage("Sender has insufficient funds.");
            });
        });

        this.When(x => x.TransactionType == TransactionType.ItemTransaction, () =>
        {
            this.RuleFor(x => x.ItemId)
                .NotEmpty();

            this.RuleFor(x => x.Item)
                .NotEmpty();

            // when the sender is not system, check if the sender has enough balance
            this.When(x => !(x.Sender?.IsSystem ?? x.SenderId == GuidExtensions.ZeroGuidValue), () =>
            {
                this.RuleFor(x => x.ItemId)
                    .Must((ctx, itemId) => ctx.Sender!.Items.Any(x => x.Id == itemId))
                    .WithMessage("You do not own this item.");
            });
        });
    }
}

public sealed class CreateTransactionCommandHandler : IResultRequestHandler<CreateTransactionCommand, bool>
{
    private readonly ITransactionWorker transactionWorker;

    public CreateTransactionCommandHandler(ITransactionWorker transactionWorker)
    {
        this.transactionWorker = transactionWorker;
    }

    public async Task<ErrorOr<bool>> Handle(CreateTransactionCommand request, CancellationToken ct)
    {
        switch (request.TransactionType)
        {
            case TransactionType.BalanceTransaction:
            {
                await this.transactionWorker.EnqueueTransaction(
                    request.Sender!,
                    request.Receiver!,
                    request.Amount!.Value, ct);

                break;
            }

            case TransactionType.ItemTransaction:
            {
                var item = request.Sender!.Items
                    .First(x => x.Id == request.ItemId!.Value);

                await this.transactionWorker.EnqueueTransaction(
                    request.Sender,
                    request.Receiver!,
                    item,
                    ct);

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(request.TransactionType), "no such transaction type");
        }

        return true;
    }
}