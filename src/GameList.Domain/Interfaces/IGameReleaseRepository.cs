using GameList.Domain.Entities;
using GameList.Domain.Enums;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de lanzamientos de juegos.
/// </summary>
public interface IGameReleaseRepository
{
    /// <summary>
    /// Devuelve los lanzamientos comprendidos en el rango de fechas indicado, con filtros opcionales.
    /// </summary>
    /// <param name="dateRange">Rango de fechas del calendario.</param>
    /// <param name="platformId">Filtro por plataforma (opcional).</param>
    /// <param name="category">Filtro por categoría de juego (opcional).</param>
    /// <param name="isIndie">Filtro por juego indie (opcional).</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de lanzamientos que cumplen los criterios.</returns>
    Task<IReadOnlyList<GameReleaseEntity>> GetByDateRangeAsync(
        DateRangeValue dateRange,
        int? platformId = null,
        GameCategoryEnum? category = null,
        bool? isIndie = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene un lanzamiento concreto por juego y plataforma.
    /// </summary>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="platformId">Identificador de la plataforma.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>El lanzamiento encontrado, o <c>null</c> si no existe.</returns>
    Task<GameReleaseEntity?> GetByGameAndPlatformAsync(
        int gameId,
        int platformId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade un nuevo lanzamiento al contexto de persistencia.
    /// </summary>
    /// <param name="release">Entidad del lanzamiento a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(GameReleaseEntity release, CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una colección de lanzamientos al contexto de persistencia.
    /// </summary>
    /// <param name="releases">Colección de lanzamientos a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddRangeAsync(IEnumerable<GameReleaseEntity> releases, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina todos los lanzamientos asociados al juego indicado.
    /// </summary>
    /// <param name="gameId">Identificador del juego cuyos lanzamientos se eliminarán.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task DeleteByGameIdAsync(int gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Busca lanzamientos por nombre de juego en un año completo.
    /// </summary>
    /// <param name="year">Año en el que buscar.</param>
    /// <param name="name">Texto parcial del nombre del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de lanzamientos que coinciden con el nombre en el año indicado.</returns>
    Task<IReadOnlyList<GameReleaseEntity>> SearchByNameAsync(
        int year,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
