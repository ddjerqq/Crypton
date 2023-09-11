using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Economy.Commands;

public sealed record SendBalanceCommand(Guid ReceiverId, decimal Amount) : IRequest<IErrorOr>;

public sealed class CreateBalanceTransactionValidator : AbstractValidator<SendBalanceCommand>
{
    public CreateBalanceTransactionValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ReceiverId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}