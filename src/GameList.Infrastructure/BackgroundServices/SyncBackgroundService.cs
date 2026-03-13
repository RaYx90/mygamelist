using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameList.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que ejecuta la sincronización de juegos desde IGDB cada 24 horas.
/// Realiza una sincronización inicial al arrancar la aplicación.
/// </summary>
public sealed class SyncBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<SyncBackgroundService> logger;
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(24);

    /// <summary>
    /// Inicializa el servicio con la fábrica de scopes y el logger.
    /// </summary>
    /// <param name="scopeFactory">Fábrica para crear scopes de inyección de dependencias.</param>
    /// <param name="logger">Logger del servicio.</param>
    public SyncBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<SyncBackgroundService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <inheritdoc/>
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
            logger.LogInformation("Sync background service stopping.");
        }
    }

    /// <summary>
    /// Crea un scope, resuelve el servicio de sincronización y lanza la sincronización del año actual.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Running scheduled game sync at {Time} UTC", DateTime.UtcNow);
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IGameSyncService>();
            await syncService.SyncAsync(DateTime.UtcNow.Year, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Scheduled game sync failed.");
        }
    }
}
