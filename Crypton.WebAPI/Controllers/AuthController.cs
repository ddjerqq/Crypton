// <copyright file="AuthController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using Crypton.Application.Auth;
using Crypton.Application.Dtos;
using Crypton.Infrastructure.Diamond;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Controllers;

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly IRules rules;
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;

    public AuthController(IRules rules, SignInManager<User> signInManager, UserManager<User> userManager)
    {
        this.rules = rules;
        this.signInManager = signInManager;
        this.userManager = userManager;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterCommand command)
    {
        var user = new User
        {
            UserName = command.Username,
            Email = command.Email,
        };

        var result = await this.signInManager.UserManager.CreateAsync(user, command.Password);

        if (result.Succeeded)
        {
            return this.Ok();
        }

        return this.BadRequest();
    }


    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command)
    {
        var result = await this.signInManager
            .PasswordSignInAsync(command.Username, command.Password, command.RememberMe, true);

        if (result.Succeeded)
        {
            var token = JwtTokenManager.GenerateToken(this.User.Claims);
            return this.Ok(token);
        }

        if (result.RequiresTwoFactor)
        {
            throw new NotImplementedException("2fa is not implemented yet");
        }

        if (result.IsLockedOut)
        {
            return this.StatusCode(StatusCodes.Status429TooManyRequests);
        }

        return this.BadRequest();
    }


    [IgnoreDigitalSignature]
    [HttpGet("rules")]
    public IActionResult Rules()
    {
        return Ok((Rules)this.rules);
    }


    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await this.userManager.GetUserAsync(this.User);

        if (user is null)
            return this.NotFound();

        return this.Ok((UserDto)user);
    }
}