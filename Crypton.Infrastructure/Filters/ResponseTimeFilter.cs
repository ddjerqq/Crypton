using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crypton.Infrastructure.Filters;

public sealed class ResponseTimeFilter : IActionFilter
{
    private const string ResponseTimeKey = "response_time";
    private const string ResponseTimeHeader = "X-Response-Time-Ms";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items[ResponseTimeKey] = Stopwatch.StartNew();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.HttpContext.Items[ResponseTimeKey] is Stopwatch stopwatch)
        {
            stopwatch.Stop();
            var elapsed = $"{stopwatch.ElapsedMilliseconds} ms";
            context.HttpContext.Response.Headers.Add(ResponseTimeHeader, elapsed);
        }
    }
}