using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Crypton.WebAPI.Filters;

public sealed class ClientIpAddressFilter : IActionFilter
{
    [SuppressMessage("Usage", "ASP0019", Justification = "This is a filter, not a controller action")]
    public void OnActionExecuting(ActionExecutingContext context)
    {
        IPAddress? ipAddress = default;

        if (TryExtractFromRealIpHeader(context.HttpContext, out var realIpAddress))
            ipAddress ??= realIpAddress;

        if (TryExtractFromForwardedForHeader(context.HttpContext, out var forwardedFromIpAddress))
            ipAddress ??= forwardedFromIpAddress;

        ipAddress ??= context.HttpContext.Connection.RemoteIpAddress;

        context.HttpContext.Response.Headers.Add("X-Client-IP", ipAddress?.ToString() ?? "undefined");
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    private static bool TryExtractFromForwardedForHeader(HttpContext httpContext, [MaybeNullWhen(false)] out IPAddress ipAddress)
    {
        ipAddress = default;

        if (!httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var xForwardedFor)
            || string.IsNullOrEmpty(xForwardedFor))
            return false;

        ipAddress = xForwardedFor
            .FirstOrDefault()!
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(ip => IPAddress.TryParse(ip, out _))
            .Select(IPAddress.Parse)
            .FirstOrDefault(address => address.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6);

        return ipAddress is not null;
    }

    private static bool TryExtractFromRealIpHeader(HttpContext httpContext, [MaybeNullWhen(false)] out IPAddress ipAddress)
    {
        ipAddress = default;

        if (!httpContext.Request.Headers.TryGetValue("X-Real-IP", out var xRealIp))
            return false;

        if (!IPAddress.TryParse(xRealIp, out ipAddress))
            return false;

        return ipAddress.AddressFamily is AddressFamily.InterNetwork or AddressFamily.InterNetworkV6;
    }
}