using Crypton.Domain.Entities;
using Crypton.Domain.ValueObjects;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Economy.Commands;

internal sealed record BalanceTransactionCommand(User? Sender, User? Receiver, decimal Amount)
    : IRequest<IErrorOr>
{
    public Wallet SenderWallet => Sender is { Wallet: var wallet } ? wallet : Wallet.NewWallet();

    public Wallet ReceiverWallet => Receiver is { Wallet: var wallet } ? wallet : Wallet.NewWallet();
}

internal sealed class BalanceTransactionValidator : AbstractValidator<BalanceTransactionCommand>
{
    public BalanceTransactionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Sender)
            .NotEmpty()
            .When(x => x.Receiver is null)
            .WithMessage("Either the sender or the receiver must be present");

        RuleFor(x => x.Receiver)
            .NotEmpty()
            .When(x => x.Sender is null)
            .WithMessage("Either the sender or the receiver must be present");

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}