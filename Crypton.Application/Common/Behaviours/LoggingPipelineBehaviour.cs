using System.Diagnostics;
using Crypton.Application.Common.Interfaces;
using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Behaviours;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly Stopwatch _stopwatch = new();
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

    public LoggingPipelineBehaviour(
        ICurrentUserAccessor currentUserAccessor,
        ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        this._currentUserAccessor = currentUserAccessor;
        this._logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var user = await this._currentUserAccessor.GetCurrentUserAsync(ct);

        this._logger.LogInformation(
            "{@UserId} {@UserName} started request {@RequestName} {@Request}",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request);

        this._stopwatch.Start();
        var result = await next();
        this._stopwatch.Stop();

        var end = this._stopwatch.ElapsedMilliseconds;
        this._stopwatch.Reset();

        this._logger.LogInformation(
            "{@UserId} {@UserName} finished request {@RequestName} {@Request} in {@Duration}ms",
            user?.Id,
            user?.UserName,
            typeof(TRequest).Name,
            request,
            end);

        return result;
    }
}