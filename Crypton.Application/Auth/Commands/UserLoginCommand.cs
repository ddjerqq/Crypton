using Crypton.Domain.Entities;
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
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.Username)
            .Length(3, 16)
            .Matches(@"^[a-zA-Z0-9._]{3,16}$")
            .WithMessage("Username must contain only alphanumeric characters, underscores and dots.");

        this.RuleFor(x => x.Password)
            .Length(5, 32)
            .NotEmpty();
    }
}

public sealed class UserLoginHandler : IRequestHandler<UserLoginCommand, ErrorOr<SignInResult>>
{
    private readonly SignInManager<User> _signInManager;

    public UserLoginHandler(SignInManager<User> signInManager)
    {
        this._signInManager = signInManager;
    }

    public async Task<ErrorOr<SignInResult>> Handle(UserLoginCommand command, CancellationToken ct)
    {
        return await this._signInManager
            .PasswordSignInAsync(
                command.Username,
                command.Password,
                command.RememberMe,
                true);
    }
}