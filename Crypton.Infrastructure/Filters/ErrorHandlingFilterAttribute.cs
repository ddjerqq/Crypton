using System.Diagnostics;
using Crypton.Infrastructure.Errors;
using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crypton.Infrastructure.Filters;

public sealed class ErrorHandlingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not CommandFailedException commandFailedException)
        {
            base.OnException(context);
            return;
        }

        var errors = commandFailedException.ErrorOr.Errors ?? new();
        var firstError = errors.FirstOrDefault();

        var statusCode = firstError.Type switch
        {
            ErrorType.Unexpected => 500,
            ErrorType.Validation or ErrorType.Failure => 400,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => throw new ArgumentOutOfRangeException(nameof(firstError), "error type not handled"),
        };

        var type = statusCode switch
        {
            400 => "https://www.rfc-editor.org/rfc/rfc9110#name-400-bad-request",
            404 => "https://www.rfc-editor.org/rfc/rfc9110#name-404-not-found",
            409 => "https://www.rfc-editor.org/rfc/rfc9110#name-409-conflict",
            500 => "https://www.rfc-editor.org/rfc/rfc9110#name-500-internal-server-error",
            _ => throw new ArgumentOutOfRangeException(nameof(statusCode), "status code not handled"),
        };

        var traceId = Activity.Current?.Id ?? context.HttpContext?.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Type = type,
            Status = statusCode,
            Title = "One or more errors have occurred.",
            Extensions =
            {
                ["errors"] = errors
                    .Select(x => new { x.Code, x.Description })
                    .ToDictionary(x => x.Code, x => x.Description),
                ["traceId"] = traceId,
            },
        };

        context.Result = new ObjectResult(problemDetails);
        context.ExceptionHandled = true;
    }
}