// <copyright file="RulePayload.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Crypton.Infrastructure.Diamond;

/// <summary>
/// this class represents the payload of each digitally signed http request.
/// </summary>
public sealed class RulePayload
{
    private RulePayload(string url, string appToken, string userId, string timestamp, string signature, ClaimsPrincipal user)
    {
        this.Url = url;
        this.AppToken = appToken;
        this.UserId = userId;
        this.Timestamp = timestamp;
        this.Signature = signature;
        this.User = user;
    }

    public string Url { get; set; }

    public string AppToken { get; set; }

    public string UserId { get; set; }

    public string Timestamp { get; set; }

    public string Signature { get; set; }

    public ClaimsPrincipal User { get; set; }

    public static RulePayload? FromHttpRequest(HttpRequest request)
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

public sealed class RulePayloadValidator : AbstractValidator<RulePayload>
{
    public RulePayloadValidator(IRules rules)
    {
        this.ClassLevelCascadeMode = CascadeMode.Stop;
        this.RuleLevelCascadeMode = CascadeMode.Stop;

        this.RuleFor(x => x.Url)
            .NotEmpty();

        this.RuleFor(x => x.AppToken)
            .NotEmpty()
            .Equal(rules.AppTokenDigest)
            .WithMessage("AppToken mismatch");

        this.RuleFor(x => x.UserId)
            .NotEmpty()
            .Must((payload, userId) => payload.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId)
            .WithMessage("UserId mismatch");

        this.RuleFor(x => x.Timestamp)
            .NotEmpty()
            .Must(ts => DateTime.TryParse(ts, out _))
            .WithMessage("Timestamp is not a valid ISO8601 date")
            .Must(ts => DateTime.Parse(ts) > DateTime.Now.AddMinutes(-5))
            .WithMessage("Timestamp is expired")
            .Must(ts => DateTime.Parse(ts) < DateTime.Now.AddMinutes(5))
            .WithMessage("Timestamp is in the future");

        this.RuleFor(x => x.Signature)
            .NotEmpty()
            .Must((payload, sign) =>
            {
                var timestamp = DateTime.TryParse(payload.Timestamp, out var dt) ? dt : (DateTime?)null;
                var expectedSignature = rules.Sign(payload.Url, payload.UserId, timestamp);

                return sign == expectedSignature;
            })
            .WithMessage("Signature mismatch");
    }
}