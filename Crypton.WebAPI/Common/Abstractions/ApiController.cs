using Crypton.Infrastructure.Errors;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.WebAPI.Common.Abstractions;

[ApiController]
[Produces("application/json")]
[Route("/api/v1/[controller]")]
public abstract class ApiController : ControllerBase
{
    protected async Task<TResponse> HandleCommandAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken ct = default)
        where TRequest : IRequest<ErrorOr<TResponse>>
    {
        var mediator = this.HttpContext.RequestServices.GetRequiredService<IMediator>();
        var result = await mediator.Send(request, ct);

        if (result.IsError)
            throw new CommandFailedException(result);

        return result.Value;
    }

    protected async Task HandleCommandAsync<TRequest>(
        TRequest request,
        CancellationToken ct = default)
        where TRequest : IRequest<IErrorOr>
    {
        var mediator = this.HttpContext.RequestServices.GetRequiredService<IMediator>();
        var result = await mediator.Send(request, ct);

        if (result.IsError)
            throw new CommandFailedException(result);
    }
}