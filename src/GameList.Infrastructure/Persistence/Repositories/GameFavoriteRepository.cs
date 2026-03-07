using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class GameFavoriteRepository : IGameFavoriteRepository
{
    private readonly AppDbContext _context;
    public GameFavoriteRepository(AppDbContext context) => _context = context;

    public Task<GameFavoriteEntity?> GetAsync(int userId, int gameId, CancellationToken ct) =>
        _context.GameFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.GameId == gameId, ct);

    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdAsync(int userId, CancellationToken ct) =>
        await _context.GameFavorites.Where(f => f.UserId == userId).ToListAsync(ct);

    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken ct) =>
        await _context.GameFavorites
            .Include(f => f.Game).ThenInclude(g => g!.Releases)
            .Where(f => userIds.Contains(f.UserId))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken ct) =>
        await _context.GameFavorites
            .Where(f => userIds.Contains(f.UserId) && gameIds.Contains(f.GameId))
            .ToListAsync(ct);

    public async Task AddAsync(GameFavoriteEntity favorite, CancellationToken ct) =>
        await _context.GameFavorites.AddAsync(favorite, ct);

    public void Remove(GameFavoriteEntity favorite) => _context.GameFavorites.Remove(favorite);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
