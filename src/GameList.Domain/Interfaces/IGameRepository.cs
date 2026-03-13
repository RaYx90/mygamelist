using GameList.Domain.Entities;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de juegos.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Obtiene un juego por su identificador interno, incluyendo sus lanzamientos.
    /// </summary>
    /// <param name="id">Identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El juego encontrado, o <c>null</c> si no existe.</returns>
    Task<GameEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un juego por su identificador de IGDB.
    /// </summary>
    /// <param name="igdbId">Identificador en la API de IGDB.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El juego encontrado, o <c>null</c> si no existe.</returns>
    Task<GameEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todos los juegos almacenados.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de todos los juegos.</returns>
    Task<IReadOnlyList<GameEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve juegos que tienen descripción en inglés pero todavía no han sido traducidos al español.
    /// </summary>
    /// <param name="limit">Número máximo de juegos a devolver.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de juegos sin traducción.</returns>
    Task<IReadOnlyList<GameEntity>> GetUntranslatedAsync(int limit, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo juego al contexto de persistencia.
    /// </summary>
    /// <param name="game">Entidad del juego a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(GameEntity game, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un juego como modificado en el contexto de persistencia.
    /// </summary>
    /// <param name="game">Entidad del juego a actualizar.</param>
    void Update(GameEntity game);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
