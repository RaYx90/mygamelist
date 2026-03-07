using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class GamePurchaseRepository : IGamePurchaseRepository
{
    private readonly AppDbContext _context;
    public GamePurchaseRepository(AppDbContext context) => _context = context;

    public Task<GamePurchaseEntity?> GetAsync(int userId, int gameId, CancellationToken ct) =>
        _context.GamePurchases.FirstOrDefaultAsync(p => p.UserId == userId && p.GameId == gameId, ct);

    public async Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken ct) =>
        await _context.GamePurchases
            .Include(p => p.Game)
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken ct) =>
        await _context.GamePurchases
            .Where(p => userIds.Contains(p.UserId) && gameIds.Contains(p.GameId))
            .ToListAsync(ct);

    public async Task AddAsync(GamePurchaseEntity purchase, CancellationToken ct) =>
        await _context.GamePurchases.AddAsync(purchase, ct);

    public void Remove(GamePurchaseEntity purchase) => _context.GamePurchases.Remove(purchase);

    public Task SaveChangesAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
}
