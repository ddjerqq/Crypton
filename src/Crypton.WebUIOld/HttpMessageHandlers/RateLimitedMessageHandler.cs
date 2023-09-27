using System.Net;

namespace Crypton.WebUIOld.HttpMessageHandlers;

public sealed class RateLimitedMessageHandler : DelegatingHandler
{
    public const int MaxTries = 5;

    private readonly ILogger<RateLimitedMessageHandler> _logger;

    public RateLimitedMessageHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<RateLimitedMessageHandler>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        HttpResponseMessage response = default!;

        for (int i = 0; i < MaxTries; i++)
        {
            response = await base.SendAsync(request, ct);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter;
                var timeToWait = retryAfter?.Delta?.Milliseconds ?? 1000;

                _logger.LogWarning("Rate limited, waiting {@TimeToWait}", timeToWait);

                await Task.Delay(timeToWait, ct);
                continue;
            }

            break;
        }

        return response;
    }
}