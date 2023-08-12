using Crypton.Application.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Common.Behaviours;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger;
    private readonly ICurrentUserAccessor currentUserAccessor;

    public LoggingPipelineBehaviour(
        ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger,
        ICurrentUserAccessor currentUserAccessor)
    {
        this.logger = logger;
        this.currentUserAccessor = currentUserAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var user = await this.currentUserAccessor.GetCurrentUserAsync(ct);

        this.logger.LogInformation(
            "{@UserId} {@UserName} started request {@RequestName} {@Request}",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request);

        var start = DateTime.UtcNow;
        var result = await next();
        var end = (DateTime.UtcNow - start).TotalMilliseconds;

        this.logger.LogInformation(
            "{@UserId} {@UserName} finished request {@RequestName} {@Request} in {@Duration}ms",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request, end);

        // if result returned an error explicitly, and didnt throw an error, we will log it
        if (result.IsError)
        {
            this.logger.LogError(
                "{@UserId} {UserName} request {@RequestName} {@Request} returned errors {@Error}",
                user?.Id,
                user?.UserName,
                typeof(TRequest).Name,
                request, result.Errors);
        }

        return result;
    }
}