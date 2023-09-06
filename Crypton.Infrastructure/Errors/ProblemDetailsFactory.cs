using System.Diagnostics;
using System.Net;
using Crypton.Domain.Common.Extensions;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Crypton.Infrastructure.Errors;

public static class ProblemDetailsFactory
{
    public static ProblemDetails CreateProblemDetails(List<Error> errors, HttpContext? httpContext = default)
    {
        var firstError = errors.FirstOrDefault();
        var statusCode = firstError.Type switch
        {
            ErrorType.Unexpected => 500,
            ErrorType.Validation or ErrorType.Failure => 400,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => throw new ArgumentOutOfRangeException(nameof(firstError), "error type not handled"),
        };

        var problemDetails = new ProblemDetails
        {
            Type = GetProblemDetailsType(statusCode),
            Status = statusCode,
            Title = "One or more errors have occured",
            Extensions =
            {
                ["errors"] = errors
                    .ToDictionary(x => x.Code.ToSnakeCase(), x => new[] { x.Description }),
            },
        };

        var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
        if (!string.IsNullOrEmpty(traceId))
            problemDetails.Extensions.Add("trace_id", traceId);

        return problemDetails;
    }

    public static string GetProblemDetailsType(int statusCode)
    {
        // get the name of the status code first
        var statusCodeName = ((HttpStatusCode)statusCode)
            .ToString()
            .ToSnakeCase()
            .Replace("_", "-");

        // https://www.rfc-editor.org/rfc/rfc9110#name-CODE-NAME
        return $"https://www.rfc-editor.org/rfc/rfc9110#name-{statusCode}-{statusCodeName}";
    }
}