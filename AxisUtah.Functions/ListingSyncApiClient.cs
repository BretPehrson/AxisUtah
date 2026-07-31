using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AxisUtah.Functions;

public class ListingSyncApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<ListingSyncApiClient> logger)
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ListingSyncApiClient> _logger = logger;

    public async Task TriggerListingSyncAsync(string correlationId, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["SyncApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("SyncApi:ApiKey must be configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "internal/sync/listings");
        ApplyInternalHeaders(request, apiKey, correlationId);

        _logger.LogInformation("Triggering API listing sync at {BaseAddress}.", _httpClient.BaseAddress);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "API listing sync trigger failed with status code {StatusCode}. Response body length={ResponseLength}.",
                (int)response.StatusCode,
                details.Length);
            throw new HttpRequestException($"Listing sync endpoint returned {(int)response.StatusCode}: {details}");
        }

        _logger.LogInformation("API listing sync trigger completed with status code {StatusCode}.", (int)response.StatusCode);
    }

    public async Task WriteLogAsync(
        string level,
        string category,
        string eventType,
        string message,
        string source,
        string correlationId,
        object? details = null,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["SyncApi:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("SyncApi:ApiKey must be configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "internal/logs")
        {
            Content = JsonContent.Create(new
            {
                level,
                category,
                eventType,
                message,
                source,
                correlationId,
                details
            })
        };

        ApplyInternalHeaders(request, apiKey, correlationId);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to persist function log entry. StatusCode={StatusCode} EventType={EventType} CorrelationId={CorrelationId}.",
                (int)response.StatusCode,
                eventType,
                correlationId);
        }
    }

    private static void ApplyInternalHeaders(HttpRequestMessage request, string apiKey, string correlationId)
    {
        request.Headers.Add("X-Sync-Api-Key", apiKey);
        request.Headers.Add("X-Correlation-Id", correlationId);
    }
}