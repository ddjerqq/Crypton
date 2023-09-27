using Crypton.Application.Common.Interfaces;
using Crypton.Infrastructure.Diamond;

// using Crypton.Infrastructure.Diamond;

namespace Crypton.WebUIOld.HttpMessageHandlers;

public sealed class DigitalSignatureMessageHandler : DelegatingHandler
{
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public DigitalSignatureMessageHandler(ICurrentUserAccessor currentUserAccessor)
    {
        _currentUserAccessor = currentUserAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var url = request.RequestUri?.ToString();
        var userId = _currentUserAccessor.GetCurrentUserId()?.ToString();
        var timestamp = DateTime.UtcNow;
        var signature = Rules.Shared.Sign(url, userId, timestamp);

        if (!string.IsNullOrEmpty(signature))
        {
            request.Headers.Add(RuleConstants.AppTokenHeaderName, Rules.Shared.AppTokenDigest);
            request.Headers.Add(RuleConstants.UserIdHeaderName, userId);
            request.Headers.Add(RuleConstants.TimestampHeaderName, $"{timestamp:yyyy-MM-dd'T'HH:mm:ss'.'fff'Z'}");
            request.Headers.Add(RuleConstants.SignatureHeaderName, signature);
        }

        Console.WriteLine(signature);

        return await base.SendAsync(request, ct);
    }
}