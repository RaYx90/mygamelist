using GameList.Domain.Entities;

namespace GameList.Domain.Ports;

public interface IGameFavoriteRepository
{
    Task<GameFavoriteEntity?> GetAsync(int userId, int gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken cancellationToken = default);
    Task AddAsync(GameFavoriteEntity favorite, CancellationToken cancellationToken = default);
    void Remove(GameFavoriteEntity favorite);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
