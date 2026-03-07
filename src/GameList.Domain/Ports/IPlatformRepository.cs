using GameList.Domain.Entities;

namespace GameList.Domain.Ports;

public interface IPlatformRepository
{
    Task<PlatformEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PlatformEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PlatformEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(PlatformEntity platform, CancellationToken cancellationToken = default);
    void Update(PlatformEntity platform);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
