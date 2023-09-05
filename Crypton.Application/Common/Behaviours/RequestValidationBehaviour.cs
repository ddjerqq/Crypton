using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ErrorOr;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ValidationException = FluentValidation.ValidationException;

namespace Crypton.Application.Common.Behaviours;

public sealed class RequestValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task
            .WhenAll(_validators.Select(v => v.ValidateAsync(context, ct)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (validationResults.All(x => x.IsValid))
        {
            return await next();
        }

        return TryCreateResponseFromErrors(failures, out var response)
            ? response
            : throw new ValidationException(failures);
    }

    private static bool TryCreateResponseFromErrors(
        List<ValidationFailure> validationFailures,
        [MaybeNullWhen(false)] [NotNullWhen(true)] out TResponse response)
    {
        List<Error> errors = validationFailures.ConvertAll(x => Error.Validation(
            code: x.PropertyName,
            description: x.ErrorMessage));

        response = (TResponse?)typeof(TResponse)
            .GetMethod(
                name: nameof(ErrorOr<object>.From),
                bindingAttr: BindingFlags.Static | BindingFlags.Public,
                types: new[] { typeof(List<Error>) })?
            .Invoke(null, new object?[] { errors })!;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        return response is not null;
    }
}