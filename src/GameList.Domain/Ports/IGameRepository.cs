using GameList.Domain.Entities;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Ports;

public interface IGameRepository
{
    Task<GameEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GameEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<GameEntity>> GetUntranslatedAsync(int limit, CancellationToken cancellationToken = default);
    Task AddAsync(GameEntity game, CancellationToken cancellationToken = default);
    void Update(GameEntity game);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
