using Crypton.Domain.Common.Errors;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Crypton.Application.Common.Behaviours;

internal sealed class ErrorHandlingBehaviour<TRequest, TResponse>
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

            // TODO: somehow limit how much info we provide about the errors if we are not in development

            // if TResponse is IErrorOr
            //   then use Errors.Unexpected("unhandled_exception", ex.Message)
            if (typeof(TResponse) == typeof(IErrorOr))
                return (dynamic)Errors.From(Error.Unexpected("unhandled_exception", ex.Message));

            // if TResponse is ErrorOr<T>
            //   then use Error.Unexpected("unhandled_exception", ex.Message)
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
                return (dynamic)Error.Unexpected("unhandled_exception", ex.Message);

            throw;
        }
    }
}