namespace AxisUtah.Functions.Functions;

public class ListingSyncTimerFunction(
    ListingSyncApiClient syncApiClient,
    ILogger<ListingSyncTimerFunction> logger)
{
    private readonly ListingSyncApiClient _syncApiClient = syncApiClient;
    private readonly ILogger<ListingSyncTimerFunction> _logger = logger;

    [Function(nameof(ListingSyncTimerFunction))]
    public async Task Run(
        [TimerTrigger("%MlsSyncSchedule%", UseMonitor = true)] TimerInfo timerInfo,
        CancellationToken cancellationToken)
    {
        var correlationId = Guid.NewGuid().ToString("n");

        if (timerInfo.IsPastDue)
        {
            _logger.LogWarning("Listing sync timer execution is running late.");
            await _syncApiClient.WriteLogAsync(
                level: "Warning",
                category: nameof(ListingSyncTimerFunction),
                eventType: "FunctionTimerPastDue",
                message: "Listing sync timer execution is running late.",
                source: "AxisUtah.Functions",
                correlationId: correlationId,
                details: new { timerInfo.ScheduleStatus?.Last, timerInfo.ScheduleStatus?.Next },
                cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Starting scheduled listing sync. Next run at {NextRun}.", timerInfo.ScheduleStatus?.Next);
        await _syncApiClient.WriteLogAsync(
            level: "Information",
            category: nameof(ListingSyncTimerFunction),
            eventType: "FunctionTimerStarted",
            message: "Starting scheduled listing sync.",
            source: "AxisUtah.Functions",
            correlationId: correlationId,
            details: new { timerInfo.ScheduleStatus?.Next },
            cancellationToken: cancellationToken);

        try
        {
            await _syncApiClient.TriggerListingSyncAsync(correlationId, cancellationToken);

            _logger.LogInformation("Scheduled listing sync completed.");
            await _syncApiClient.WriteLogAsync(
                level: "Information",
                category: nameof(ListingSyncTimerFunction),
                eventType: "FunctionTimerCompleted",
                message: "Scheduled listing sync completed.",
                source: "AxisUtah.Functions",
                correlationId: correlationId,
                cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled listing sync failed.");
            await _syncApiClient.WriteLogAsync(
                level: "Error",
                category: nameof(ListingSyncTimerFunction),
                eventType: "FunctionTimerFailed",
                message: ex.Message,
                source: "AxisUtah.Functions",
                correlationId: correlationId,
                details: new { exception = ex.ToString() },
                cancellationToken: cancellationToken);
            throw;
        }
    }
}