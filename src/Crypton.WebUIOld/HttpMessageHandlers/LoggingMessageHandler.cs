using System.Net.Http.Json;

namespace Crypton.WebUIOld.HttpMessageHandlers;

public sealed class LoggingMessageHandler : DelegatingHandler
{
    private readonly ILogger<LoggingMessageHandler> _logger;

    public LoggingMessageHandler(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LoggingMessageHandler>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        ProblemDetails? problemDetails = default!;

        _logger.LogInformation(
            "Requesting {Method} {Uri}",
            request.Method,
            request.RequestUri?.ToString() ?? "null");

        var response = await base.SendAsync(request, ct);

        if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
        {
            // try get the ProblemDetails out of the response
            try
            {
                problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            }
            catch (Exception)
            {
                // ignore
                return response;
            }
        }

        if (problemDetails is not null)
        {
            var detail = string.Join(
                '\n',
                problemDetails
                    .Errors
                    .Select(error =>
                        $"{error.Key} {string.Join(',', error.Value)}"));

            _logger.LogError(
                "Client error: {StatusCode} {ReasonPhrase} {Title} {Detail}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                problemDetails.Title,
                detail);
        }
        else
        {
            _logger.LogError("Client error: {StatusCode} {ReasonPhrase}", (int)response.StatusCode, response.ReasonPhrase);
        }

        return response;
    }
}