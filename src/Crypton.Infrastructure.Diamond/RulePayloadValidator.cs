using System.Security.Claims;
using FluentValidation;

namespace Crypton.Infrastructure.Diamond;

public sealed class RulePayloadValidator : AbstractValidator<RulePayload>
{
    public RulePayloadValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Url)
            .NotEmpty();

        RuleFor(x => x.AppToken)
            .NotEmpty()
            .Equal(Rules.Shared.AppTokenDigest)
            .WithMessage("AppToken mismatch");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .Must((payload, userId) => payload.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value == userId)
            .WithMessage("UserId mismatch");

        RuleFor(x => x.Timestamp)
            .NotEmpty()
            .Must(ts => DateTime.TryParse(ts, out _))
            .WithMessage("Timestamp is not a valid ISO8601 date")
            .Must(ts => DateTime.Parse(ts).ToUniversalTime() > DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("Timestamp is expired")
            .Must(ts => DateTime.Parse(ts).ToUniversalTime() < DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp is in the future");

        RuleFor(x => x.Signature)
            .NotEmpty()
            .Must((payload, sign) =>
            {
                var timestamp = DateTime.TryParse(payload.Timestamp, out var dt) ? dt : (DateTime?)null;
                var expectedSignature = Rules.Shared.Sign(payload.Url, payload.UserId, timestamp);

                return sign == expectedSignature;
            })
            .WithMessage("Signature mismatch");
    }
}