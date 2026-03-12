using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class GameRepository : IGameRepository
{
    private readonly AppDbContext _context;

    public GameRepository(AppDbContext context) => _context = context;

    public async Task<GameEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Games
            .Include(g => g.Releases)
                .ThenInclude(r => r.Platform)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    public async Task<GameEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default) =>
        await _context.Games
            .FirstOrDefaultAsync(g => g.IgdbId == igdbId, cancellationToken);

    public async Task<IReadOnlyList<GameEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Games
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<GameEntity>> GetUntranslatedAsync(int limit, CancellationToken cancellationToken = default) =>
        await _context.Games
            .Where(g => g.Summary != null && g.Summary != "" && (g.SummaryEs == null || g.SummaryEs == ""))
            .OrderBy(g => g.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(GameEntity game, CancellationToken cancellationToken = default) =>
        await _context.Games.AddAsync(game, cancellationToken);

    public void Update(GameEntity game) =>
        _context.Games.Update(game);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
