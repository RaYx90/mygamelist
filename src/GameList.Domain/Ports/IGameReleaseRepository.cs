using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Ports;

public interface IGameReleaseRepository
{
    Task<IReadOnlyList<GameReleaseEntity>> GetByDateRangeAsync(
        DateRangeValue dateRange,
        int? platformId = null,
        GameCategoryEnum? category = null,
        bool? isIndie = null,
        CancellationToken cancellationToken = default);

    Task<GameReleaseEntity?> GetByGameAndPlatformAsync(
        int gameId,
        int platformId,
        CancellationToken cancellationToken = default);

    Task AddAsync(GameReleaseEntity release, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<GameReleaseEntity> releases, CancellationToken cancellationToken = default);
    Task DeleteByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
