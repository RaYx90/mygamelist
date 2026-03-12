using GameList.Domain.Ports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameList.Infrastructure.BackgroundServices;

public sealed class TranslationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TranslationBackgroundService> _logger;
    private static readonly TimeSpan BatchInterval = TimeSpan.FromSeconds(15);
    private const int BatchSize = 20;

    public TranslationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TranslationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
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
            _logger.LogInformation("Translation background service stopping.");
        }
    }

    private async Task TranslateBatchAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var gameRepository = scope.ServiceProvider.GetRequiredService<IGameRepository>();
            var translationService = scope.ServiceProvider.GetRequiredService<ITranslationService>();

            var games = await gameRepository.GetUntranslatedAsync(BatchSize, cancellationToken);
            if (games.Count == 0) return;

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
                await gameRepository.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Translated {Count} game summaries", translated);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Translation batch failed.");
        }
    }
}
