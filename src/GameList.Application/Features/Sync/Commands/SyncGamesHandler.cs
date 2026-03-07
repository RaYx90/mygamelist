using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.Ports;
using GameList.Domain.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GameList.Application.Features.Sync.Commands;

public sealed class SyncGamesHandler : IRequestHandler<SyncGamesCommand, SyncResultDto>
{
    private readonly IGameDataProvider _dataProvider;
    private readonly IGameRepository _gameRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly IGameReleaseRepository _releaseRepository;
    private readonly ILogger<SyncGamesHandler> _logger;

    public SyncGamesHandler(
        IGameDataProvider dataProvider,
        IGameRepository gameRepository,
        IPlatformRepository platformRepository,
        IGameReleaseRepository releaseRepository,
        ILogger<SyncGamesHandler> logger)
    {
        _dataProvider = dataProvider;
        _gameRepository = gameRepository;
        _platformRepository = platformRepository;
        _releaseRepository = releaseRepository;
        _logger = logger;
    }

    public async Task<SyncResultDto> Handle(
        SyncGamesCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting game sync for year {Year}", request.Year);

        try
        {
            var releaseData = await _dataProvider.GetReleasesForYearAsync(
                request.Year, cancellationToken);

            // 1 — Upsert platforms
            var platformCache = new Dictionary<long, PlatformEntity>();
            foreach (var data in releaseData.DistinctBy(d => d.IgdbPlatformId))
            {
                var existing = await _platformRepository.GetByIgdbIdAsync(
                    data.IgdbPlatformId, cancellationToken);

                if (existing is null)
                {
                    existing = PlatformEntity.Create(
                        data.PlatformName,
                        data.PlatformSlug,
                        data.IgdbPlatformId,
                        data.PlatformAbbreviation);
                    await _platformRepository.AddAsync(existing, cancellationToken);
                }
                else
                {
                    existing.UpdateAbbreviation(data.PlatformAbbreviation);
                    _platformRepository.Update(existing);
                }

                platformCache[data.IgdbPlatformId] = existing;
            }

            await _platformRepository.SaveChangesAsync(cancellationToken);

            // 2 — Upsert games
            var gameCache = new Dictionary<long, GameEntity>();
            foreach (var data in releaseData.DistinctBy(d => d.IgdbGameId))
            {
                var existing = await _gameRepository.GetByIgdbIdAsync(
                    data.IgdbGameId, cancellationToken);

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
                    await _gameRepository.AddAsync(existing, cancellationToken);
                }
                else
                {
                    existing.Update(data.GameName, data.Summary, data.CoverImageUrl, data.GameCategory, data.IsIndie);
                    _gameRepository.Update(existing);
                }

                gameCache[data.IgdbGameId] = existing;
            }

            await _gameRepository.SaveChangesAsync(cancellationToken);

            // 3 — Rebuild releases (delete + re-insert for simplicity)
            var processedGameIds = gameCache.Values.Select(g => g.Id).ToList();
            foreach (var gameId in processedGameIds)
                await _releaseRepository.DeleteByGameIdAsync(gameId, cancellationToken);

            // Group by game to determine exclusive vs multiplatform
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

                    await _releaseRepository.AddAsync(
                        GameReleaseEntity.Create(
                            gameId,
                            platformId,
                            data.ReleaseDate,
                            releaseType,
                            data.Region),
                        cancellationToken);
                }
            }

            await _releaseRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Sync completed. Games: {Games}, Platforms: {Platforms}",
                gameCache.Count,
                platformCache.Count);

            return new SyncResultDto(
                Success: true,
                GamesProcessed: gameCache.Count,
                PlatformsProcessed: platformCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sync failed for year {Year}", request.Year);
            return new SyncResultDto(
                Success: false,
                GamesProcessed: 0,
                PlatformsProcessed: 0,
                ErrorMessage: ex.Message);
        }
    }
}
