using System.Net;

namespace Crypton.WebUIOld.HttpMessageHandlers;

public sealed class IdempotentMessageHandler : DelegatingHandler
{
    private readonly ILogger<IdempotentMessageHandler> _logger;

    public IdempotentMessageHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IdempotentMessageHandler>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put || request.Method == HttpMethod.Patch)
        {
            var idempotencyKey = Guid.NewGuid();
            request.Headers.Add("X-Idempotency-Key", idempotencyKey.ToString());
        }

        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            _logger.LogError("Idempotency error");
        }

        return response;
    }
}