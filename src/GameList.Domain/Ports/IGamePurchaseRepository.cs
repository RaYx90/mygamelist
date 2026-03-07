using GameList.Domain.Entities;

namespace GameList.Domain.Ports;

public interface IGamePurchaseRepository
{
    Task<GamePurchaseEntity?> GetAsync(int userId, int gameId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken cancellationToken = default);
    Task AddAsync(GamePurchaseEntity purchase, CancellationToken cancellationToken = default);
    void Remove(GamePurchaseEntity purchase);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
