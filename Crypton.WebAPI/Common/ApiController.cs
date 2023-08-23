using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Common;

/// <summary>
/// Base controller for all API controllers.
/// </summary>
[ApiController]
[Produces("application/json")]
[Route("/api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected readonly ILogger Logger;
    protected readonly IMediator Mediator;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiController"/> class.
    /// </summary>
    protected ApiController(ILogger logger, IMediator mediator)
    {
        this.Mediator = mediator;
        this.Logger = logger;
    }

    protected virtual async Task TryHandleCommandAsync<TRequest>(
        TRequest command,
        CancellationToken ct = default)
        where TRequest : IRequest
    {
    }

    /// <summary>
    /// Tries to handle a command and returns the result.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <typeparam name="TRequest">The request.</typeparam>
    /// <typeparam name="TResponse">The response.</typeparam>
    /// <returns>Response or null</returns>
    protected virtual async Task<TResponse?> TryHandleCommandAsync<TRequest, TResponse>(
        TRequest command,
        CancellationToken ct = default)
        where TRequest : IRequest<TResponse>
    {
        TResponse response;

        try
        {
            response = await this.Mediator.Send(command, ct);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error handling command {Command}", command);
            return default;
        }

        return response;
    }
}