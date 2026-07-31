namespace AxisUtah.Api.Services;

public class ListingSyncBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<ListingSyncBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(30);
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<ListingSyncBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = GetSyncInterval();

        await RunSyncCycleAsync(stoppingToken);

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunSyncCycleAsync(stoppingToken);
        }
    }

    private TimeSpan GetSyncInterval()
    {
        var intervalMinutes = _configuration.GetValue<int?>("MlsSync:IntervalMinutes").GetValueOrDefault(30);

        return intervalMinutes > 0
            ? TimeSpan.FromMinutes(intervalMinutes)
            : DefaultInterval;
    }

    private async Task RunSyncCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var coordinator = scope.ServiceProvider.GetRequiredService<ListingSyncCoordinator>();

        try
        {
            await coordinator.RunPropertySyncAsync(cancellationToken: cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background MLS sync cycle failed.");
        }
    }
}