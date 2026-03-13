using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de juegos.
/// </summary>
internal sealed class GameRepository : IGameRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public GameRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Obtiene un juego por su identificador interno, cargando sus lanzamientos y plataformas asociadas.
    /// </summary>
    /// <param name="id">Identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El juego encontrado, o <c>null</c> si no existe.</returns>
    public async Task<GameEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        await context.Games
            .Include(g => g.Releases)
                .ThenInclude(r => r.Platform)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    /// <summary>
    /// Obtiene un juego por su identificador de IGDB.
    /// </summary>
    /// <param name="igdbId">Identificador en IGDB.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El juego encontrado, o <c>null</c> si no existe.</returns>
    public async Task<GameEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default) =>
        await context.Games
            .FirstOrDefaultAsync(g => g.IgdbId == igdbId, cancellationToken);

    /// <summary>
    /// Devuelve todos los juegos sin rastreo de cambios.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de todos los juegos.</returns>
    public async Task<IReadOnlyList<GameEntity>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Games
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Devuelve los primeros <paramref name="limit"/> juegos con descripción en inglés pero sin traducción al español.
    /// </summary>
    /// <param name="limit">Número máximo de juegos a devolver.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de juegos sin traducción.</returns>
    public async Task<IReadOnlyList<GameEntity>> GetUntranslatedAsync(int limit, CancellationToken cancellationToken = default) =>
        await context.Games
            .Where(g => g.Summary != null && g.Summary != "" && (g.SummaryEs == null || g.SummaryEs == ""))
            .OrderBy(g => g.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Añade un nuevo juego al contexto.
    /// </summary>
    /// <param name="game">Entidad del juego a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task AddAsync(GameEntity game, CancellationToken cancellationToken = default) =>
        await context.Games.AddAsync(game, cancellationToken);

    /// <summary>
    /// Marca un juego como modificado en el contexto.
    /// </summary>
    /// <param name="game">Entidad del juego a actualizar.</param>
    public void Update(GameEntity game) =>
        context.Games.Update(game);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await context.SaveChangesAsync(cancellationToken);
}
