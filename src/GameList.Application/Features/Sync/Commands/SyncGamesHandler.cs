using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.Interfaces;
using GameList.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameList.Application.Features.Sync.Commands;

/// <summary>
/// Sincroniza los juegos desde IGDB para el año indicado.
/// Toda la operación de BD se ejecuta dentro de una transacción atómica para evitar
/// que los usuarios vean estados intermedios (ej: releases borrados sin reinsertar).
/// </summary>
/// <remarks>
/// Fase 1 — Upsert de plataformas:
///   Se crean o actualizan las plataformas encontradas en los datos de IGDB.
///   Se usa un diccionario (platformCache) para evitar consultas repetidas a la BD.
///
/// Fase 2 — Upsert de juegos:
///   Se crean o actualizan los juegos. La traducción al español NO ocurre aquí;
///   la maneja TranslationBackgroundService de forma asíncrona en segundo plano.
///
/// Fase 3 — Reconstrucción de releases:
///   Se borran todos los releases existentes del año y se reinsertan desde cero.
///   Esto simplifica el manejo de cambios en IGDB (plataformas añadidas/eliminadas, etc.)
///   y determina si un juego es exclusivo (1 plataforma) o multiplataforma (>1 plataformas).
/// </remarks>
public sealed class SyncGamesHandler : IRequestHandler<SyncGamesCommand, SyncResultDto>
{
    private readonly IGameDataProvider dataProvider;
    private readonly IGameRepository gameRepository;
    private readonly IPlatformRepository platformRepository;
    private readonly IGameReleaseRepository releaseRepository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<SyncGamesHandler> logger;

    public SyncGamesHandler(
        IGameDataProvider dataProvider,
        IGameRepository gameRepository,
        IPlatformRepository platformRepository,
        IGameReleaseRepository releaseRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncGamesHandler> logger)
    {
        this.dataProvider = dataProvider;
        this.gameRepository = gameRepository;
        this.platformRepository = platformRepository;
        this.releaseRepository = releaseRepository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<SyncResultDto> Handle(
        SyncGamesCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Iniciando sync de juegos para el año {Year}", request.Year);

        try
        {
            // Obtener datos de IGDB fuera de la transacción (operación de red, puede tardar).
            var releaseData = await dataProvider.GetReleasesForYearAsync(
                request.Year, cancellationToken);

            // Variables para capturar contadores dentro de la lambda de transacción.
            var platformCount = 0;
            var gameCount = 0;

            // Transacción atómica: los usuarios nunca ven un estado intermedio.
            // Sin esto, durante el borrado+reinserción de releases (~1-2 min),
            // la web mostraría días sin juegos.
            // Se usa ExecuteInTransactionAsync para compatibilidad con NpgsqlRetryingExecutionStrategy.
            await unitOfWork.ExecuteInTransactionAsync(async ct =>
            {
                // FASE 1 — Upsert de plataformas
                var platformCache = new Dictionary<long, PlatformEntity>();
                foreach (var data in releaseData.DistinctBy(d => d.IgdbPlatformId))
                {
                    var existing = await platformRepository.GetByIgdbIdAsync(
                        data.IgdbPlatformId, ct);

                    if (existing is null)
                    {
                        existing = PlatformEntity.Create(
                            data.PlatformName,
                            data.PlatformSlug,
                            data.IgdbPlatformId,
                            data.PlatformAbbreviation);
                        await platformRepository.AddAsync(existing, ct);
                    }
                    else
                    {
                        existing.UpdateAbbreviation(data.PlatformAbbreviation);
                        platformRepository.Update(existing);
                    }

                    platformCache[data.IgdbPlatformId] = existing;
                }

                await platformRepository.SaveChangesAsync(ct);

                // FASE 2 — Upsert de juegos
                var gameCache = new Dictionary<long, GameEntity>();
                foreach (var data in releaseData.DistinctBy(d => d.IgdbGameId))
                {
                    var existing = await gameRepository.GetByIgdbIdAsync(
                        data.IgdbGameId, ct);

                    if (existing is null)
                    {
                        existing = GameEntity.Create(
                            data.GameName,
                            data.GameSlug,
                            data.IgdbGameId,
                            data.Summary,
                            data.CoverImageUrl,
                            data.GameCategory,
                            data.IsIndie);
                        await gameRepository.AddAsync(existing, ct);
                    }
                    else
                    {
                        existing.Update(data.GameName, data.Summary, data.CoverImageUrl, data.GameCategory, data.IsIndie);
                        gameRepository.Update(existing);
                    }

                    gameCache[data.IgdbGameId] = existing;
                }

                await gameRepository.SaveChangesAsync(ct);

                // FASE 3 — Reconstrucción de releases (borrar + reinsertar)
                var processedGameIds = gameCache.Values.Select(g => g.Id).ToList();
                foreach (var gameId in processedGameIds)
                    await releaseRepository.DeleteByGameIdAsync(gameId, ct);

                var releasesByGame = releaseData.GroupBy(d => d.IgdbGameId);

                foreach (var group in releasesByGame)
                {
                    var releaseType = group.DistinctBy(d => d.IgdbPlatformId).Count() == 1
                        ? ReleaseTypeEnum.Exclusive
                        : ReleaseTypeEnum.Multiplatform;

                    foreach (var data in group.DistinctBy(d => d.IgdbPlatformId))
                    {
                        var gameId = gameCache[data.IgdbGameId].Id;
                        var platformId = platformCache[data.IgdbPlatformId].Id;

                        await releaseRepository.AddAsync(
                            GameReleaseEntity.Create(
                                gameId,
                                platformId,
                                data.ReleaseDate,
                                releaseType,
                                data.Region),
                            ct);
                    }
                }

                await releaseRepository.SaveChangesAsync(ct);

                // Capturar contadores para el log fuera de la lambda.
                platformCount = platformCache.Count;
                gameCount = gameCache.Count;
            }, cancellationToken);

            logger.LogInformation(
                "Sync completado. Juegos: {Games}, Plataformas: {Platforms}",
                gameCount,
                platformCount);

            return new SyncResultDto(
                Success: true,
                GamesProcessed: gameCount,
                PlatformsProcessed: platformCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Sync failed for year {Year}", request.Year);
            return new SyncResultDto(
                Success: false,
                GamesProcessed: 0,
                PlatformsProcessed: 0,
                ErrorMessage: ex.Message);
        }
    }
}
