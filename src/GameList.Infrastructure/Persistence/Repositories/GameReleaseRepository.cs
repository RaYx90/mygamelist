using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.Ports;
using GameList.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class GameReleaseRepository : IGameReleaseRepository
{
    private readonly AppDbContext _context;

    public GameReleaseRepository(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<GameReleaseEntity>> GetByDateRangeAsync(
        DateRangeValue dateRange,
        int? platformId = null,
        GameCategoryEnum? category = null,
        bool? isIndie = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.GameReleases
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.Platform)
            .Where(r => r.ReleaseDate >= dateRange.Start && r.ReleaseDate <= dateRange.End);

        if (platformId.HasValue)
            query = query.Where(r => r.PlatformId == platformId.Value);

        if (category.HasValue)
            query = query.Where(r => r.Game!.Category == category.Value);

        if (isIndie.HasValue)
            query = query.Where(r => r.Game!.IsIndie == isIndie.Value);

        return await query
            .OrderBy(r => r.ReleaseDate)
            .ThenBy(r => r.Game!.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<GameReleaseEntity?> GetByGameAndPlatformAsync(
        int gameId,
        int platformId,
        CancellationToken cancellationToken = default) =>
        await _context.GameReleases
            .FirstOrDefaultAsync(
                r => r.GameId == gameId && r.PlatformId == platformId,
                cancellationToken);

    public async Task AddAsync(GameReleaseEntity release, CancellationToken cancellationToken = default) =>
        await _context.GameReleases.AddAsync(release, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<GameReleaseEntity> releases,
        CancellationToken cancellationToken = default) =>
        await _context.GameReleases.AddRangeAsync(releases, cancellationToken);

    public async Task DeleteByGameIdAsync(int gameId, CancellationToken cancellationToken = default) =>
        await _context.GameReleases
            .Where(r => r.GameId == gameId)
            .ExecuteDeleteAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
