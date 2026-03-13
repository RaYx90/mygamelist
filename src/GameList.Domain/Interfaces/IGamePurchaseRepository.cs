using GameList.Domain.Entities;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de juegos marcados como comprados.
/// </summary>
public interface IGamePurchaseRepository
{
    /// <summary>
    /// Obtiene el registro de compra de un usuario para un juego concreto.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El registro de compra encontrado, o <c>null</c> si no existe.</returns>
    Task<GamePurchaseEntity?> GetAsync(int userId, int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los registros de compra de una lista de usuarios, incluyendo datos del juego.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de compras de los usuarios indicados.</returns>
    Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve los registros de compra de una lista de usuarios filtrados por juegos concretos.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="gameIds">Lista de identificadores de juegos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Compras que coinciden con los usuarios y juegos indicados.</returns>
    Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo registro de compra al contexto de persistencia.
    /// </summary>
    /// <param name="purchase">Entidad de compra a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(GamePurchaseEntity purchase, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un registro de compra del contexto de persistencia.
    /// </summary>
    /// <param name="purchase">Entidad de compra a eliminar.</param>
    void Remove(GamePurchaseEntity purchase);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
