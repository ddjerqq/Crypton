using System.Text.Json.Serialization;
using Crypton.Application.Common.Abstractions;
using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using ErrorOr;
using FluentValidation;

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
    [JsonIgnore]
    public string SenderId { get; set; } = string.Empty;

    [JsonIgnore]
    public User? Sender { get; set; }

    public bool IsSenderSystem => this.Sender?.IsSystem ?? this.SenderId == GuidExtensions.ZeroGuidValue;

    public string ReceiverId { get; set; } = string.Empty;

    [JsonIgnore]
    public User? Receiver { get; set; }

    public bool IsReceiverSystem => this.Receiver?.IsSystem ?? this.ReceiverId == GuidExtensions.ZeroGuidValue;

    public TransactionType TransactionType { get; set; }

    public decimal? Amount { get; set; }

    public Guid? ItemId { get; set; }

    [JsonIgnore]
    public Item? Item { get; set; }
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

    public async Task<ErrorOr<bool>> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        switch (request.TransactionType)
        {
            case TransactionType.BalanceTransaction:
            {
                await this.transactionWorker.EnqueueTransaction(request.Sender!, request.Receiver!, request.Amount!.Value, cancellationToken);
                break;
            }

            case TransactionType.ItemTransaction:
            {
                var item = request.Sender!.Items.First(x => x.Id == request.ItemId!.Value);
                await this.transactionWorker.EnqueueTransaction(request.Sender, request.Receiver!, item, cancellationToken);
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(request.TransactionType), "no such transaction type");
        }

        return true;
    }
}