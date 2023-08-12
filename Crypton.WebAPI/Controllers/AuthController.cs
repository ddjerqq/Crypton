using Crypton.Application.Auth;
using Crypton.Application.Dtos;
using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Diamond;
using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI.Controllers;

/// <summary>
/// Auth controller, for handling authentication / authorization, registration, logging in and digital signature rules.
/// </summary>
[Authorize]
[ApiController]
[Produces("application/json")]
[Route("/api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IRules rules;
    private readonly IAppDbContext dbContext;
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="rules">DigitalSignatureRules.</param>
    /// <param name="dbContext">AppDatabaseContext.</param>
    /// <param name="signInManager">Identity.SignInManager.</param>
    /// <param name="userManager">Identity.UserManager.</param>
    public AuthController(
        IRules rules,
        IAppDbContext dbContext,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        this.rules = rules;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <param name="command">the register command.</param>
    /// <returns>status code 201 if the registration was successful, otherwise status code 400 and IdentityErrors.</returns>
    // TODO limit these, so authenticated users cannot spam these endpoints
    [IgnoreDigitalSignature]
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
            return this.Created();
        }

        return this.BadRequest(result.Errors);
    }

    /// <summary>
    /// Login a user.
    /// </summary>
    /// <param name="command">login command.</param>
    /// <returns>status code 200 and string jwt token if the login was successful, otherwise status code 400.</returns>
    /// <exception cref="NotImplementedException">raised if identity needs 2 factor authentication.</exception>
    // TODO limit these, so authenticated users cannot spam these endpoints
    [IgnoreDigitalSignature]
    [AllowAnonymous]
    [Produces<string>]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginCommand command)
    {
        var result = await this.signInManager
            .PasswordSignInAsync(
                command.Username,
                command.Password,
                command.RememberMe,
                true);

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

        return this.BadRequest("Invalid Credentials");
    }

    /// <summary>
    /// Get current digital signature rules.
    /// </summary>
    /// <returns><see cref="Rules"/> digital signature rules.</returns>
    [IgnoreDigitalSignature]
    [Produces<Rules>]
    [HttpGet("rules")]
    public IActionResult Rules()
    {
        return this.Ok((Rules)this.rules);
    }

    /// <summary>
    /// get currently authenticated user's information.
    /// </summary>
    /// <returns><see cref="UserDto"/> the user information.</returns>
    [Produces<UserDto>]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await this.userManager.GetUserAsync(this.User);

        if (user is null)
            return this.NotFound();

        return this.Ok((UserDto)user);
    }

    /// <summary>
    /// get all users.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <returns>a list of all registered users, converted into user dtos.</returns>
    [Produces<IEnumerable<UserDto>>]
    [HttpGet("users")]
    public async Task<IActionResult> AllUsers(CancellationToken ct = default)
    {
        var users = await this.dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id != GuidExtensions.ZeroGuidValue)
            .ToListAsync(ct);

        return this.Ok(users.Select(x => (UserDto)x));
    }
}