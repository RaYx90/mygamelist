using GameList.Domain.Entities;
using GameList.Domain.Ports;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

internal sealed class PlatformRepository : IPlatformRepository
{
    private readonly AppDbContext _context;

    public PlatformRepository(AppDbContext context) => _context = context;

    public async Task<PlatformEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await _context.Platforms.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<PlatformEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default) =>
        await _context.Platforms.FirstOrDefaultAsync(p => p.IgdbId == igdbId, cancellationToken);

    public async Task<IReadOnlyList<PlatformEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _context.Platforms
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(PlatformEntity platform, CancellationToken cancellationToken = default) =>
        await _context.Platforms.AddAsync(platform, cancellationToken);

    public void Update(PlatformEntity platform) =>
        _context.Platforms.Update(platform);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);
}
