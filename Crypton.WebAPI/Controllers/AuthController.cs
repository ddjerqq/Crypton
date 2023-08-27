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

[Authorize]
[ApiController]
[Produces("application/json")]
[Route("/api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IRules _rules;
    private readonly IAppDbContext _dbContext;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public AuthController(
        IRules rules,
        IAppDbContext dbContext,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        this._rules = rules;
        this._signInManager = signInManager;
        this._userManager = userManager;
        this._dbContext = dbContext;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <response code="201">Success</response>
    /// <response code="400">Username / email / password issues</response>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody, BindRequired] UserRegisterCommand command)
    {
        var user = new User
        {
            UserName = command.Username,
            Email = command.Email,
        };

        var result = await this._signInManager.UserManager.CreateAsync(user, command.Password);

        if (result.Succeeded)
        {
            return this.Created();
        }

        return this.BadRequest(result.Errors);
    }

    /// <summary>
    /// Login a user.
    /// </summary>
    /// <response code="200">Success and JWT</response>
    /// <response code="400">Invalid Credentials</response>
    /// <response code="429">Rate Limit or Lockout</response>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("login")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Login([FromBody, BindRequired] UserLoginCommand command)
    {
        var result = await this._signInManager
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
    // TODO some way to deliver these rules to the client, but in secret.
    [AllowAnonymous]
    [IgnoreDigitalSignature]
    [HttpGet("rules")]
    [ProducesResponseType<Rules>(StatusCodes.Status200OK)]
    public IActionResult Rules()
    {
        return this.Ok((Rules)this._rules);
    }

    /// <summary>
    /// get currently authenticated user's information.
    /// </summary>
    /// <response code="200">Success and <see cref="UserDto">user info</see></response>
    [HttpGet("me")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var user = await this._userManager.GetUserAsync(this.User);

        if (user is null)
            return this.Unauthorized();

        return this.Ok((UserDto)user);
    }

    /// <summary>
    /// get all users.
    /// </summary>
    /// <response code="200">Success and <see cref="UserDto">all users' info</see></response>
    [HttpGet("users")]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AllUsers(CancellationToken ct = default)
    {
        var users = await this._dbContext.Users
            .Where(x => x.Id != GuidExtensions.ZeroGuid)
            .ToListAsync(ct);

        return this.Ok(users.Select(x => (UserDto)x));
    }
}