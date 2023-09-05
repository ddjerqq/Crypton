using Crypton.Domain.Common.Errors;
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
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling request {@Request}", request);
            return (dynamic)Errors.From(Error.Unexpected("unhandled_exception", ex.Message));
        }
    }
}