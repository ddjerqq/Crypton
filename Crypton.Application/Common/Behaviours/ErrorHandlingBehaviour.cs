using ErrorOr;
using MediatR;

namespace Crypton.Application.Common.Behaviours;

public sealed class ErrorHandlingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly ILogger<ErrorHandlingBehaviour<TRequest, TResponse>> _logger;

    public ErrorHandlingBehaviour(ILogger<ErrorHandlingBehaviour<TRequest, TResponse>> logger)
    {
        this._logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            this._logger.LogError(ex, "Error handling request {@Request}", request);
            return (dynamic)Error.Unexpected("unhandled_exception", ex.Message);
        }
    }
}