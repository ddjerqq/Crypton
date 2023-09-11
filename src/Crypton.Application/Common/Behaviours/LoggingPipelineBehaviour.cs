using System.Diagnostics;
using Crypton.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Common.Behaviours;

internal sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

    public LoggingPipelineBehaviour(
        ICurrentUserAccessor currentUserAccessor,
        ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        _currentUserAccessor = currentUserAccessor;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var user = await _currentUserAccessor.GetCurrentUserAsync(ct);

        _logger.LogInformation(
            "{@UserId} {@UserName} started request {@RequestName} {@Request}",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request);

        var stopwatch = Stopwatch.StartNew();
        var result = await next();
        stopwatch.Stop();

        var end = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "{@UserId} {@UserName} finished request {@RequestName} {@Request} in {@Duration}ms",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request,
            end);

        return result;
    }
}