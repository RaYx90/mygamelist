using GameList.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameList.Infrastructure.BackgroundServices;

/// <summary>
/// Servicio en segundo plano que traduce automáticamente las descripciones de juegos del inglés al español.
/// Se ejecuta de forma independiente al sync para no saturar LibreTranslate con ~7000 juegos a la vez.
/// </summary>
/// <remarks>
/// Parámetros de control de carga:
/// - <c>BatchSize = 20</c>: juegos traducidos por tick. Valor bajo para no saturar LibreTranslate.
/// - <c>BatchInterval = 15s</c>: espera entre ticks. Permite a LibreTranslate recuperarse entre lotes.
///
/// Por qué usa IServiceScopeFactory:
/// Los BackgroundServices son Singleton, pero DbContext e ITranslationService tienen lifetime Scoped.
/// Para evitar el error "Cannot resolve scoped service from singleton", se crea un scope nuevo
/// por cada tick del timer y se resuelven los servicios dentro de ese scope.
/// </remarks>
public sealed class TranslationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<TranslationBackgroundService> logger;

    // Intervalo entre lotes — suficiente para que LibreTranslate no se sature.
    private static readonly TimeSpan BatchInterval = TimeSpan.FromSeconds(15);

    // Número de juegos por lote — equilibrio entre velocidad y carga de CPU en el contenedor de traducción.
    private const int BatchSize = 20;

    public TranslationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TranslationBackgroundService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(BatchInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await TranslateBatchAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Se lanza al cancelar el token de parada — es el comportamiento esperado al apagar la app.
            logger.LogInformation("Servicio de traducción en segundo plano detenido.");
        }
    }

    /// <summary>
    /// Obtiene hasta <c>BatchSize</c> juegos sin traducir, los traduce en un solo request
    /// a LibreTranslate y persiste los resultados en la BD.
    /// Si LibreTranslate falla, el error se registra y se omite el lote (se reintentará en el siguiente tick).
    /// </summary>
    private async Task TranslateBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Scope por tick — necesario porque DbContext tiene lifetime Scoped (ver remarks de la clase).
            await using var scope = scopeFactory.CreateAsyncScope();
            var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
            var translationService = scope.ServiceProvider.GetRequiredService<ITranslationService>();

            // Juegos con Summary en inglés pero sin SummaryEs todavía.
            var games = await gameRepository.GetUntranslatedAsync(BatchSize, cancellationToken);
            if (games.Count == 0) return; // Nada que traducir — salida rápida.

            // Envía todos los summaries en un solo request batch a LibreTranslate.
            var translations = await translationService.TranslateBatchAsync(
                games.Select(g => g.Summary!).ToList(), "ES", cancellationToken);

            int translated = 0;
            for (int i = 0; i < games.Count && i < translations.Count; i++)
            {
                if (translations[i] is not null)
                {
                    games[i].UpdateSummaryEs(translations[i]);
                    gameRepository.Update(games[i]);
                    translated++;
                }
            }

            if (translated > 0)
            {
                // Un solo SaveChangesAsync por tick en lugar de uno por juego.
                await gameRepository.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Traducidos {Count} summaries de juegos", translated);
            }
        }
        catch (Exception ex)
        {
            // Fallo silencioso: se registra el error pero el servicio sigue corriendo.
            // El lote fallido se reintentará en el siguiente tick (15s).
            logger.LogError(ex, "Fallo en el lote de traducción.");
        }
    }
}
