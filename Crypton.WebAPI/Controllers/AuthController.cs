using Crypton.Application.Auth;
using Crypton.Application.Dtos;
using Crypton.Application.Interfaces;
using Crypton.Domain.Common.Extensions;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Diamond;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    /// <param name="command">the register command</param>
    /// <response code="201">Success</response>
    /// <response code="400">Username / email / password issues</response>
    /// <response code="429">Rate Limit</response>
    /// <response code="500">Internal Server Error</response>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody, BindRequired] UserRegisterCommand command)
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
    /// <exception cref="NotImplementedException">raised if identity needs 2 factor authentication.</exception>
    /// <response code="200">Success and JWT</response>
    /// <response code="400">Invalid Credentials</response>
    /// <response code="429">Rate Limit or Lockout</response>
    /// <response code="500">Internal Server Error</response>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("login")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody, BindRequired] UserLoginCommand command)
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
    /// <response code="200">Success and <see cref="Rules">Rules</see></response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Rate Limit</response>
    /// <response code="500">Internal Server Error</response>
    // TODO some way to deliver these rules to the client, but in secret.
    [AllowAnonymous]
    [IgnoreDigitalSignature]
    [HttpGet("rules")]
    [ProducesResponseType<Rules>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult Rules()
    {
        return this.Ok((Rules)this.rules);
    }

    /// <summary>
    /// get currently authenticated user's information.
    /// </summary>
    /// <response code="200">Success and <see cref="UserDto">user info</see></response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Rate Limit</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Me()
    {
        var user = await this.userManager.GetUserAsync(this.User);

        if (user is null)
            return this.Unauthorized();

        return this.Ok((UserDto)user);
    }

    /// <summary>
    /// get all users.
    /// </summary>
    /// <param name="ct">CancellationToken.</param>
    /// <response code="200">Success and <see cref="UserDto">all users' info</see></response>
    /// <response code="401">Unauthorized</response>
    /// <response code="429">Rate Limit</response>
    /// <response code="500">Internal Server Error</response>
    [HttpGet("users")]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AllUsers(CancellationToken ct = default)
    {
        var users = await this.dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id != GuidExtensions.ZeroGuid)
            .ToListAsync(ct);

        return this.Ok(users.Select(x => (UserDto)x));
    }
}