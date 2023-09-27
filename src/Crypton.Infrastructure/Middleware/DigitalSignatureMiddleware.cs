using Crypton.Infrastructure.Diamond;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

namespace Crypton.Infrastructure.Middleware;

public sealed class DigitalSignatureMiddleware : IMiddleware
{
    private readonly ILogger<DigitalSignatureMiddleware> _logger;
    private readonly IValidator<RulePayload> _payloadValidator;

    public DigitalSignatureMiddleware(
        ILogger<DigitalSignatureMiddleware> logger,
        IValidator<RulePayload> payloadValidator)
    {
        _logger = logger;
        _payloadValidator = payloadValidator;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var endpoint = context.GetEndpoint();
        var attributes = endpoint?.Metadata.OfType<IgnoreDigitalSignatureAttribute>();
        var hasAllowAnonymous = attributes?.Any() ?? true;

        if (hasAllowAnonymous)
        {
            await next(context);
            return;
        }

        var payload = FromHttpRequest(context.Request);

        if (payload is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        // ReSharper disable once MethodHasAsyncOverload
        // because we dont have any async validations
        if (_payloadValidator.Validate(payload) is { IsValid: false } result)
        {
            var errors = string.Join(", ", result.Errors);
            _logger.LogWarning("Digital signature issue(s): {Errors}", errors);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await next(context);
    }

    private static RulePayload? FromHttpRequest(HttpRequest request)
    {
        if (request.HttpContext.User is not { } user)
            return null;

        var uri = request.GetDisplayUrl();

        if (!request.Headers.TryGetValue(RuleConstants.AppTokenHeaderName, out var appToken)
            || string.IsNullOrEmpty(appToken)
            || !request.Headers.TryGetValue(RuleConstants.UserIdHeaderName, out var userId)
            || string.IsNullOrEmpty(userId)
            || !request.Headers.TryGetValue(RuleConstants.TimestampHeaderName, out var timestamp)
            || string.IsNullOrEmpty(timestamp)
            || !request.Headers.TryGetValue(RuleConstants.SignatureHeaderName, out var signature)
            || string.IsNullOrEmpty(signature))
            return null;

        return new RulePayload(uri, appToken!, userId!, timestamp!, signature!, user);
    }
}