// <copyright file="UserLoginCommand.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Crypton.Application.Common.Abstractions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Crypton.Application.Auth;

public sealed class UserLoginCommand : IResultRequest<SignInResult>
{
    [Required]
    [Length(3, 16)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}

public sealed class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.Username)
            .Matches(@"^[a-zA-Z0-9._]{3,16}$")
            .When(x => !string.IsNullOrEmpty(x.Username));

        this.RuleFor(x => x.Password)
            .NotEmpty();
    }
}