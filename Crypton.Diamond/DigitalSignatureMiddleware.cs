// <copyright file="DigitalSignatureMiddleware.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Crypton.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Crypton.Diamond;

public sealed class DigitalSignatureMiddleware : IMiddleware
{
    private readonly ILogger<DigitalSignatureMiddleware> logger;
    private readonly IRules rules;
    private readonly IValidator<RulePayload> payloadValidator;
    private readonly UserManager<User> userManager;

    public DigitalSignatureMiddleware(
        ILogger<DigitalSignatureMiddleware> logger,
        IRules rules,
        IValidator<RulePayload> payloadValidator,
        UserManager<User> userManager)
    {
        this.logger = logger;
        this.rules = rules;
        this.payloadValidator = payloadValidator;
        this.userManager = userManager;
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

        if (payloadValidator.Validate(payload) is {IsValid: false} result)
        {
            var errors = string.Join(", ", result.Errors);
            this.logger.LogWarning($"Digital signature issue(s): {errors}");

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        await next(context);
    }
}