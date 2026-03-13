using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de juegos favoritos de usuarios.
/// </summary>
internal sealed class GameFavoriteRepository : IGameFavoriteRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public GameFavoriteRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Obtiene el registro de favorito de un usuario para un juego concreto.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El favorito encontrado, o <c>null</c> si no existe.</returns>
    public Task<GameFavoriteEntity?> GetAsync(int userId, int gameId, CancellationToken ct) =>
        context.GameFavorites.FirstOrDefaultAsync(f => f.UserId == userId && f.GameId == gameId, ct);

    /// <summary>
    /// Devuelve todos los favoritos de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de favoritos del usuario.</returns>
    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdAsync(int userId, CancellationToken ct) =>
        await context.GameFavorites.Where(f => f.UserId == userId).ToListAsync(ct);

    /// <summary>
    /// Devuelve los favoritos de una lista de usuarios, incluyendo datos del juego y sus lanzamientos.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de favoritos con datos del juego incluidos.</returns>
    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken ct) =>
        await context.GameFavorites
            .Include(f => f.Game).ThenInclude(g => g!.Releases)
            .Where(f => userIds.Contains(f.UserId))
            .ToListAsync(ct);

    /// <summary>
    /// Devuelve los favoritos de una lista de usuarios filtrados por un conjunto de juegos.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="gameIds">Lista de identificadores de juegos.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Favoritos que coinciden con los usuarios y juegos indicados.</returns>
    public async Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken ct) =>
        await context.GameFavorites
            .Where(f => userIds.Contains(f.UserId) && gameIds.Contains(f.GameId))
            .ToListAsync(ct);

    /// <summary>
    /// Añade un nuevo favorito al contexto.
    /// </summary>
    /// <param name="favorite">Entidad del favorito a añadir.</param>
    /// <param name="ct">Token de cancelación.</param>
    public async Task AddAsync(GameFavoriteEntity favorite, CancellationToken ct) =>
        await context.GameFavorites.AddAsync(favorite, ct);

    /// <summary>
    /// Elimina un favorito del contexto.
    /// </summary>
    /// <param name="favorite">Entidad del favorito a eliminar.</param>
    public void Remove(GameFavoriteEntity favorite) => context.GameFavorites.Remove(favorite);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
