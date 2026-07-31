namespace AxisUtah.Api.Services;

public class ListingSyncCoordinator(
    AppDbContext db,
    AppLogService appLogService,
    ODataSyncService syncService,
    IConfiguration configuration,
    ILogger<ListingSyncCoordinator> logger)
{
    private readonly AppDbContext _db = db;
    private readonly AppLogService _appLogService = appLogService;
    private readonly ODataSyncService _syncService = syncService;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ListingSyncCoordinator> _logger = logger;

    public async Task RunPropertySyncAsync(string? correlationId = null, CancellationToken cancellationToken = default)
    {
        var runStartedAt = DateTimeOffset.UtcNow;
        var checkpoint = await _db.SyncCheckpoints
            .SingleOrDefaultAsync(c => c.FeedName == SyncCheckpoint.PropertyFeedName, cancellationToken);

        if (checkpoint == null)
        {
            checkpoint = new SyncCheckpoint { FeedName = SyncCheckpoint.PropertyFeedName };
            _db.SyncCheckpoints.Add(checkpoint);
        }

        checkpoint.LastRunStartedAt = DateTimeOffset.UtcNow;
        checkpoint.LastError = null;
        await _db.SaveChangesAsync(cancellationToken);

        var initialLookbackHours = Math.Max(_configuration.GetValue<int?>("MlsSync:InitialLookbackHours") ?? 24, 1);
        var syncFrom = checkpoint.LastModificationTimestamp ?? DateTimeOffset.UtcNow.AddHours(-initialLookbackHours);

        try
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                ["FeedName"] = SyncCheckpoint.PropertyFeedName,
                ["SyncFrom"] = syncFrom
            });

            _logger.LogInformation("Starting MLS sync cycle.");
            await _appLogService.WriteAsync(
                level: "Information",
                category: nameof(ListingSyncCoordinator),
                eventType: "PropertySyncStarted",
                message: "Starting MLS sync cycle.",
                details: new { syncFrom },
                source: "AxisUtah.Api",
                correlationId: correlationId,
                cancellationToken: cancellationToken);

            var latestTimestamp = await _syncService.SyncListingsAsync(syncFrom, correlationId, cancellationToken);

            if (latestTimestamp.HasValue &&
                (!checkpoint.LastModificationTimestamp.HasValue || latestTimestamp > checkpoint.LastModificationTimestamp))
            {
                checkpoint.LastModificationTimestamp = latestTimestamp;
            }

            checkpoint.LastRunCompletedAt = DateTimeOffset.UtcNow;
            checkpoint.LastError = null;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "MLS sync cycle completed successfully. LatestTimestamp={LatestTimestamp} DurationMs={DurationMs}.",
                checkpoint.LastModificationTimestamp,
                (checkpoint.LastRunCompletedAt.Value - runStartedAt).TotalMilliseconds);
            await _appLogService.WriteAsync(
                level: "Information",
                category: nameof(ListingSyncCoordinator),
                eventType: "PropertySyncCompleted",
                message: "MLS sync cycle completed successfully.",
                details: new
                {
                    checkpoint.LastModificationTimestamp,
                    durationMs = (checkpoint.LastRunCompletedAt.Value - runStartedAt).TotalMilliseconds
                },
                source: "AxisUtah.Api",
                correlationId: correlationId,
                cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            checkpoint.LastError = ex.Message;
            checkpoint.LastRunCompletedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                ex,
                "MLS sync cycle failed after {DurationMs} ms.",
                (checkpoint.LastRunCompletedAt.Value - runStartedAt).TotalMilliseconds);
            await _appLogService.WriteAsync(
                level: "Error",
                category: nameof(ListingSyncCoordinator),
                eventType: "PropertySyncFailed",
                message: ex.Message,
                details: new
                {
                    exception = ex.ToString(),
                    durationMs = (checkpoint.LastRunCompletedAt.Value - runStartedAt).TotalMilliseconds
                },
                source: "AxisUtah.Api",
                correlationId: correlationId,
                cancellationToken: cancellationToken);
            throw;
        }
    }
}