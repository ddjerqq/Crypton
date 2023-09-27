using Crypton.Application.Auth.Commands;
using Crypton.Application.Common.Interfaces;
using Crypton.Application.Dto;
using Crypton.Domain.Common.Errors;
using Crypton.Domain.Entities;
using Crypton.Domain.Events;
using Crypton.Infrastructure.Errors;
using Crypton.Infrastructure.Idempotency;
using Crypton.Infrastructure.Services;
using Crypton.WebAPI.Common.Abstractions;
using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Crypton.WebAPI.Controllers;

public sealed class AuthController : ApiController
{
    private readonly IAppDbContext _dbContext;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthController(
        IAppDbContext dbContext,
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Register([FromBody, BindRequired] UserRegisterCommand command, CancellationToken ct)
    {
        var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;
        if (isUserAuthenticated)
            return Forbid();

        var user = command.User;
        var result = await _userManager.CreateAsync(user, command.Password);

        if (result.Succeeded)
        {
            user.AddDomainEvent(new UserCreatedEvent(user.Id));
            await _dbContext.SaveChangesAsync(ct);
            return Ok();
        }

        var errors = result.Errors
            .Select(x => Error.Failure(x.Code, x.Description))
            .ToList();

        var error = Errors.From(errors);
        throw new CommandFailedException(error);
    }

    /// <summary>
    /// Login a user.
    /// </summary>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("login")]
    // TODO change every typed response from ProducesResponseType attribute
    // to return ActionFilter<T>
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody, BindRequired] UserLoginCommand command)
    {
        var isUserAuthenticated = User.Identity?.IsAuthenticated ?? false;
        if (isUserAuthenticated)
            return Forbid();

        var result = await _signInManager
            .PasswordSignInAsync(
                command.Username,
                command.Password,
                true,
                true);

        if (result.Succeeded)
            return Ok(JwtTokenManager.GenerateToken(User.Claims));

        if (result.RequiresTwoFactor)
            throw new NotImplementedException("2fa is not implemented yet");

        if (result.IsLockedOut)
            return StatusCode(StatusCodes.Status429TooManyRequests);

        return BadRequest();
    }

    /// <summary>
    /// Login as any user.
    /// </summary>
    [AllowAnonymous]
    [RequireIdempotency]
    [HttpPost("login_as/{username:required}")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LoginAs([FromRoute] string username)
    {
        if (!HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment())
            return NotFound();

        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
            return NotFound();

        await _signInManager
            .SignInAsync(user, true);

        return Ok(JwtTokenManager.GenerateToken(User.Claims));
    }

    /// <summary>
    /// Sign out
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok();
    }

    /// <summary>
    /// get currently authenticated user's information.
    /// </summary>
    /// <response code="200">Success and <see cref="UserDto">user info</see></response>
    [Authorize]
    [HttpGet("user")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUser(CancellationToken ct)
    {
        var userId = Guid.Parse(_userManager.GetUserId(User)!);

        var user = await _dbContext.Set<User>()
            .Include(x => x.Inventory)
            .ThenInclude(x => x.ItemType)
            .Include(x => x.DailyStreak)
            .FirstOrDefaultAsync(x => x.Id == userId, ct);

        return Ok((UserDto)user!);
    }

    /// <summary>
    /// get currently authenticated user's information.
    /// </summary>
    /// <response code="200">Success and <see cref="Dictionary{String,String}">user claims</see></response>
    [Authorize]
    [HttpGet("user_claims")]
    [ProducesResponseType<Dictionary<string, string>>(StatusCodes.Status200OK)]
    public IActionResult GetUserClaims()
    {
        var claims = User.Claims.ToDictionary(x => x.Type, x => x.Value);
        return Ok(claims);
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
        var users = await _dbContext.Set<User>()
            .Include(x => x.Inventory)
            .ThenInclude(x => x.ItemType)
            .Include(x => x.DailyStreak)
            .ToListAsync(ct);

        return Ok(users.Select(x => (UserDto)x));
    }
}