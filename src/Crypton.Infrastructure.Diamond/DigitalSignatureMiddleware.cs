﻿using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crypton.Infrastructure.Diamond;

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

        var payload = RulePayload.FromHttpRequest(context.Request);

        if (payload is null)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        if (await _payloadValidator.ValidateAsync(payload) is { IsValid: false } result)
        {
            var errors = string.Join(", ", result.Errors);
            _logger.LogWarning("Digital signature issue(s): {Errors}", errors);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await next(context);
    }
}