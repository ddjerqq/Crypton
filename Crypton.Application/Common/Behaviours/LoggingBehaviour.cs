using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Common.Behaviours;

public sealed class LoggingPipelineBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> _logger;

    public LoggingPipelineBehaviour(ILogger<LoggingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _logger.LogInformation("started request {@RequestName} {@Request}", typeof(TRequest).Name, request);

        var start = DateTime.UtcNow;
        var result = await next();
        double end = (DateTime.UtcNow - start).TotalMilliseconds;

        _logger.LogInformation("finished request {@RequestName} {@Request} in {@Duration}ms", typeof(TRequest).Name,
            request, end);

        // if result returned an error explicitly, and didnt throw an error, we will log it
        if (result.IsError)
            _logger.LogError("request {@RequestName} {@Request} returned errors {@Error}", typeof(TRequest).Name,
                request, result.Errors);

        return result;
    }
}