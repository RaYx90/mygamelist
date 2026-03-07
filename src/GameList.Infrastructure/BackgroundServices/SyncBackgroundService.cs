using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameList.Infrastructure.BackgroundServices;

public sealed class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SyncBackgroundService> _logger;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);

    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initial sync on startup
        await RunSyncAsync(stoppingToken);

        using var timer = new PeriodicTimer(SyncInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunSyncAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Sync background service stopping.");
        }
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running scheduled game sync at {Time} UTC", DateTime.UtcNow);
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IGameSyncService>();
            await syncService.SyncAsync(DateTime.UtcNow.Year, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled game sync failed.");
        }
    }
}
