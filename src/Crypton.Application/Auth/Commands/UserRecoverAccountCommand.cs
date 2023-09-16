using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Auth.Commands;

public sealed record UserRecoverCommand(string Email)
    : IRequest<IErrorOr>
{
    public string Email { get; set; } = Email;
}

public sealed class UserRecoverValidator : AbstractValidator<UserRecoverCommand>
{
    public UserRecoverValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Email)
            .NotEmpty()
            .Matches(@"^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$")
            .WithMessage("Email must be a valid email address.");
    }
}