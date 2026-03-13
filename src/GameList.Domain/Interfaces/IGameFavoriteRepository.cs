using GameList.Domain.Entities;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de juegos favoritos de usuarios.
/// </summary>
public interface IGameFavoriteRepository
{
    /// <summary>
    /// Obtiene el registro de favorito de un usuario para un juego concreto.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El favorito encontrado, o <c>null</c> si no existe.</returns>
    Task<GameFavoriteEntity?> GetAsync(int userId, int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todos los favoritos de un usuario.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de favoritos del usuario.</returns>
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los favoritos de una lista de usuarios, incluyendo datos del juego.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de favoritos de los usuarios indicados.</returns>
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los favoritos de una lista de usuarios filtrados por juegos concretos.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="gameIds">Lista de identificadores de juegos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Favoritos que coinciden con los usuarios y juegos indicados.</returns>
    Task<IReadOnlyList<GameFavoriteEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo favorito al contexto de persistencia.
    /// </summary>
    /// <param name="favorite">Entidad del favorito a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(GameFavoriteEntity favorite, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un favorito del contexto de persistencia.
    /// </summary>
    /// <param name="favorite">Entidad del favorito a eliminar.</param>
    void Remove(GameFavoriteEntity favorite);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
