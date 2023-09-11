using System.Text.Json.Serialization;
using Crypton.Domain.Entities;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace Crypton.Application.Auth.Commands;

public sealed record UserRegisterCommand(string Username, string Email, string Password) : IRequest<IErrorOr>
{
    [JsonIgnore]
    public User User => new User
    {
        Id = Guid.NewGuid(),
        UserName = Username,
        Email = Email,
    };
}

public sealed class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9._]{3,16}$")
            .WithMessage(
                "Username must be between 3 and 16 characters long and contain only alphanumeric characters, underscores and dots.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .Matches(@"^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$")
            .WithMessage("Email must be a valid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage(
                "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.");
    }
}