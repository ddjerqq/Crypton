using System.Text.Json.Serialization;
using Crypton.Application.Common.Interfaces;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Domain.Events;
using ErrorOr;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;

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

internal sealed class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
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

internal sealed class UserRegisterHandler : IRequestHandler<UserRegisterCommand, IErrorOr>
{
    private readonly IAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public UserRegisterHandler(UserManager<User> userManager, IAppDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<IErrorOr> Handle(UserRegisterCommand request, CancellationToken ct)
    {
        var user = request.User;
        var res = await _userManager.CreateAsync(user, request.Password);

        if (res.Succeeded)
        {
            user.AddDomainEvent(new UserCreatedEvent(user.Id));
            await _dbContext.SaveChangesAsync(ct);
            return Errors.Success;
        }

        var errors = res.Errors
            .Select(x => Error.Failure(x.Code.ToSnakeCase(), x.Description))
            .ToList();

        return Errors.From(errors);
    }
}