using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de plataformas.
/// </summary>
internal sealed class PlatformRepository : IPlatformRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public PlatformRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Obtiene una plataforma por su identificador interno.
    /// </summary>
    /// <param name="id">Identificador interno de la plataforma.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La plataforma encontrada, o <c>null</c> si no existe.</returns>
    public async Task<PlatformEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Platforms.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    /// <summary>
    /// Obtiene una plataforma por su identificador de IGDB.
    /// </summary>
    /// <param name="igdbId">Identificador en IGDB.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La plataforma encontrada, o <c>null</c> si no existe.</returns>
    public async Task<PlatformEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default) =>
        await context.Platforms.FirstOrDefaultAsync(p => p.IgdbId == igdbId, cancellationToken);

    /// <summary>
    /// Devuelve todas las plataformas sin rastreo de cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de todas las plataformas.</returns>
    public async Task<IReadOnlyList<PlatformEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Platforms
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Añade una nueva plataforma al contexto.
    /// </summary>
    /// <param name="platform">Entidad de la plataforma a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task AddAsync(PlatformEntity platform, CancellationToken cancellationToken = default) =>
        await context.Platforms.AddAsync(platform, cancellationToken);

    /// <summary>
    /// Marca una plataforma como modificada en el contexto.
    /// </summary>
    /// <param name="platform">Entidad de la plataforma a actualizar.</param>
    public void Update(PlatformEntity platform) =>
        context.Platforms.Update(platform);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
