using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.Interfaces;
using GameList.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de lanzamientos de juegos.
/// </summary>
internal sealed class GameReleaseRepository : IGameReleaseRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public GameReleaseRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Devuelve los lanzamientos dentro del rango de fechas indicado, con filtros opcionales por plataforma, categoría e indie.
    /// </summary>
    /// <param name="dateRange">Rango de fechas del calendario.</param>
    /// <param name="platformId">Filtro por plataforma (opcional).</param>
    /// <param name="category">Filtro por categoría de juego (opcional).</param>
    /// <param name="isIndie">Filtro por juego indie (opcional).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de lanzamientos ordenados por fecha y nombre de juego.</returns>
    public async Task<IReadOnlyList<GameReleaseEntity>> GetByDateRangeAsync(
        DateRangeValue dateRange,
        int? platformId = null,
        GameCategoryEnum? category = null,
        bool? isIndie = null,
        CancellationToken cancellationToken = default)
    {
        var query = context.GameReleases
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

    /// <summary>
    /// Obtiene un lanzamiento concreto por juego y plataforma.
    /// </summary>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="platformId">Identificador de la plataforma.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El lanzamiento encontrado, o <c>null</c> si no existe.</returns>
    public async Task<GameReleaseEntity?> GetByGameAndPlatformAsync(
        int gameId,
        int platformId,
        CancellationToken cancellationToken = default) =>
        await context.GameReleases
            .FirstOrDefaultAsync(
                r => r.GameId == gameId && r.PlatformId == platformId,
                cancellationToken);

    /// <summary>
    /// Añade un nuevo lanzamiento al contexto.
    /// </summary>
    /// <param name="release">Entidad del lanzamiento a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task AddAsync(GameReleaseEntity release, CancellationToken cancellationToken = default) =>
        await context.GameReleases.AddAsync(release, cancellationToken);

    /// <summary>
    /// Añade una colección de lanzamientos al contexto.
    /// </summary>
    /// <param name="releases">Colección de lanzamientos a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task AddRangeAsync(
        IEnumerable<GameReleaseEntity> releases,
        CancellationToken cancellationToken = default) =>
        await context.GameReleases.AddRangeAsync(releases, cancellationToken);

    /// <summary>
    /// Elimina directamente en la base de datos todos los lanzamientos del juego indicado.
    /// </summary>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task DeleteByGameIdAsync(int gameId, CancellationToken cancellationToken = default) =>
        await context.GameReleases
            .Where(r => r.GameId == gameId)
            .ExecuteDeleteAsync(cancellationToken);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
