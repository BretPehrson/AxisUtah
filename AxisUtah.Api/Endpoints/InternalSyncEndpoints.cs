namespace AxisUtah.Api.Endpoints;

public static class InternalSyncEndpoints
{
    public static void MapInternalSyncEndpoints(this WebApplication app)
    {
        app.MapPost("/internal/sync/listings", HandleListingSync);
        app.MapPost("/internal/logs", HandleLogs);
        app.MapGet("/admin/logs", GetLogs);
    }

    private static async Task<IResult> HandleListingSync(
        HttpRequest request,
        ListingSyncCoordinator coordinator,
        AppLogService appLogService,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("InternalSyncEndpoint");
        var expectedApiKey = configuration["SyncAutomation:ApiKey"];
        var correlationId = request.Headers["X-Correlation-Id"].ToString();
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("n");
        }

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            logger.LogError("Sync automation endpoint called before SyncAutomation:ApiKey was configured.");
            await appLogService.WriteAsync(
                level: "Error",
                category: "InternalSyncEndpoint",
                eventType: "SyncConfigurationMissing",
                message: "Sync automation endpoint called before SyncAutomation:ApiKey was configured.",
                source: "AxisUtah.Api",
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            return Results.Problem(
                detail: "Sync automation key is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (!request.Headers.TryGetValue("X-Sync-Api-Key", out var providedApiKey) ||
            !string.Equals(providedApiKey.ToString(), expectedApiKey, StringComparison.Ordinal))
        {
            logger.LogWarning("Rejected internal listing sync request due to invalid automation key.");
            await appLogService.WriteAsync(
                level: "Warning",
                category: "InternalSyncEndpoint",
                eventType: "SyncAuthorizationFailed",
                message: "Rejected internal listing sync request due to invalid automation key.",
                source: "AxisUtah.Api",
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            return Results.Unauthorized();
        }

        logger.LogInformation("Accepted internal listing sync request.");
        await appLogService.WriteAsync(
            level: "Information",
            category: "InternalSyncEndpoint",
            eventType: "SyncRequestAccepted",
            message: "Accepted internal listing sync request.",
            source: "AxisUtah.Api",
            correlationId: correlationId,
            cancellationToken: cancellationToken);

        await coordinator.RunPropertySyncAsync(correlationId, cancellationToken);

        logger.LogInformation("Internal listing sync request completed successfully.");
        await appLogService.WriteAsync(
            level: "Information",
            category: "InternalSyncEndpoint",
            eventType: "SyncRequestCompleted",
            message: "Internal listing sync request completed successfully.",
            source: "AxisUtah.Api",
            correlationId: correlationId,
            cancellationToken: cancellationToken);

        return Results.Ok(new { status = "completed" });
    }

    private static async Task<IResult> HandleLogs(
        HttpRequest request,
        LogEntryDto logEntry,
        AppLogService appLogService,
        IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var expectedApiKey = configuration["SyncAutomation:ApiKey"];

        if (string.IsNullOrWhiteSpace(expectedApiKey))
        {
            return Results.Problem(
                detail: "Sync automation key is not configured.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        if (!request.Headers.TryGetValue("X-Sync-Api-Key", out var providedApiKey) ||
            !string.Equals(providedApiKey.ToString(), expectedApiKey, StringComparison.Ordinal))
        {
            return Results.Unauthorized();
        }

        await appLogService.WriteAsync(
            logEntry.Level,
            logEntry.Category,
            logEntry.EventType,
            logEntry.Message,
            logEntry.Details,
            logEntry.Source,
            logEntry.CorrelationId,
            cancellationToken);

        return Results.Accepted();
    }

    private static async Task<IResult> GetLogs(
        AppLogService appLogService,
        string? level,
        string? source,
        string? eventType,
        string? correlationId,
        string? search,
        DateTimeOffset? createdAfterUtc,
        DateTimeOffset? createdBeforeUtc,
        int? skip,
        int? take,
        CancellationToken cancellationToken)
    {
        var entries = await appLogService.QueryAsync(
            level,
            source,
            eventType,
            correlationId,
            search,
            createdAfterUtc,
            createdBeforeUtc,
            skip ?? 0,
            take ?? 100,
            cancellationToken);

        return Results.Ok(entries);
    }
}