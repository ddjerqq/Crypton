using Crypton.Infrastructure.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crypton.WebAPI.Filters;

public sealed class ErrorHandlingFilterAttribute : ExceptionFilterAttribute
{
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not CommandFailedException commandFailedException)
        {
            base.OnException(context);
            return;
        }

        var problemDetails = ProblemDetailsFactory.CreateProblemDetails(
            commandFailedException.ErrorOr.Errors!,
            context.HttpContext);

        context.Result = new ObjectResult(problemDetails);
        context.ExceptionHandled = true;
    }
}