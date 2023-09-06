using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Application.Auth.Commands;

public sealed record UserLoginCommand(string Username, string Password, bool RememberMe)
    : IRequest<ErrorOr<SignInResult>>;

public sealed class UserLoginValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .Length(3, 16)
            .Matches(@"^[a-zA-Z0-9._]{3,16}$")
            .WithMessage("Username must contain only alphanumeric characters, underscores and dots.");

        RuleFor(x => x.Password)
            .Length(5, 32)
            .NotEmpty();
    }
}