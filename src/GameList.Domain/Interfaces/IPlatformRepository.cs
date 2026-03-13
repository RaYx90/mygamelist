using GameList.Domain.Entities;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para el acceso y persistencia de plataformas.
/// </summary>
public interface IPlatformRepository
{
    /// <summary>
    /// Obtiene una plataforma por su identificador interno.
    /// </summary>
    /// <param name="id">Identificador interno de la plataforma.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La plataforma encontrada, o <c>null</c> si no existe.</returns>
    Task<PlatformEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene una plataforma por su identificador de IGDB.
    /// </summary>
    /// <param name="igdbId">Identificador en la API de IGDB.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La plataforma encontrada, o <c>null</c> si no existe.</returns>
    Task<PlatformEntity?> GetByIgdbIdAsync(long igdbId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todas las plataformas almacenadas.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de todas las plataformas.</returns>
    Task<IReadOnlyList<PlatformEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Añade una nueva plataforma al contexto de persistencia.
    /// </summary>
    /// <param name="platform">Entidad de la plataforma a añadir.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task AddAsync(PlatformEntity platform, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca una plataforma como modificada en el contexto de persistencia.
    /// </summary>
    /// <param name="platform">Entidad de la plataforma a actualizar.</param>
    void Update(PlatformEntity platform);

    /// <summary>
    /// Persiste todos los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
