using Crypton.Application.Auth.Commands;
using Crypton.Application.Common.Interfaces;
using Crypton.Application.Dtos;
using Crypton.Domain.Entities;
using Crypton.Infrastructure.Diamond;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Services;
using Crypton.WebAPI.Common.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Crypton.WebAPI.Controllers;

public sealed class AuthController : ApiController
{
    private readonly IRules _rules;
    private readonly IAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;

    public AuthController(
        IRules rules,
        IAppDbContext dbContext,
        UserManager<User> userManager)
    {
        this._rules = rules;
        this._userManager = userManager;
        this._dbContext = dbContext;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Identity issues</response>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody, BindRequired] UserRegisterCommand command)
    {
        await this.HandleCommandAsync(command);
        return this.Ok();
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
        var result = await this.HandleCommandAsync<UserLoginCommand, SignInResult>(command);

        if (result.Succeeded)
            return this.Ok(JwtTokenManager.GenerateToken(this.User.Claims));

        if (result.RequiresTwoFactor)
            throw new NotImplementedException("2fa is not implemented yet");

        if (result.IsLockedOut)
            return this.StatusCode(StatusCodes.Status429TooManyRequests);

        return this.BadRequest();
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
    [Authorize]
    [HttpGet("user")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser()
    {
        var user = await this._userManager.GetUserAsync(this.User);
        return this.Ok((UserDto)user!);
    }

    /// <summary>
    /// get all users.
    /// </summary>
    /// <response code="200">Success and <see cref="UserDto">all users' info</see></response>
    [Authorize]
    [HttpGet("users")]
    [ProducesResponseType<IEnumerable<UserDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> AllUsers(CancellationToken ct)
    {
        var users = await this._dbContext.Set<User>()
            .ToListAsync(ct);

        return this.Ok(users.Select(x => (UserDto)x));
    }
}