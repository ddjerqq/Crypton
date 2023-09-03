﻿using System.Text.Json.Serialization;
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

public sealed class UserRegisterCommand : IRequest<IErrorOr>
{
    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    [JsonIgnore]
    public User User => new User
    {
        Id = Guid.NewGuid(),
        UserName = this.Username,
        Email = this.Email,
    };
}

public sealed class UserRegisterValidator : AbstractValidator<UserRegisterCommand>
{
    public UserRegisterValidator()
    {
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.Username)
            .NotEmpty()
            .Matches(@"^[a-zA-Z0-9._]{3,16}$")
            .WithMessage(
                "Username must be between 3 and 16 characters long and contain only alphanumeric characters, underscores and dots.");

        this.RuleFor(x => x.Email)
            .NotEmpty()
            .Matches(@"^[\w-.]+@([\w-]+\.)+[\w-]{2,4}$")
            .WithMessage("Email must be a valid email address.");

        this.RuleFor(x => x.Password)
            .NotEmpty()
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")
            .WithMessage(
                "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number and one special character.");
    }
}

public sealed class UserRegisterHandler : IRequestHandler<UserRegisterCommand, IErrorOr>
{
    private readonly IAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public UserRegisterHandler(UserManager<User> userManager, IAppDbContext dbContext)
    {
        this._userManager = userManager;
        this._dbContext = dbContext;
    }

    public async Task<IErrorOr> Handle(UserRegisterCommand request, CancellationToken ct)
    {
        var user = request.User;
        var res = await this._userManager.CreateAsync(user, request.Password);

        if (res.Succeeded)
        {
            user.AddDomainEvent(new UserCreatedEvent(user.Id));
            await this._dbContext.SaveChangesAsync(ct);
            return Errors.Success;
        }

        var errors = res.Errors
            .Select(x => Error.Failure(x.Code.ToSnakeCase(), x.Description))
            .ToList();

        return Errors.From(errors);
    }
}