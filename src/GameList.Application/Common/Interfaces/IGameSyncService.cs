namespace GameList.Application.Common.Interfaces;

/// <summary>
/// Servicio de aplicación responsable de sincronizar los juegos desde la fuente externa.
/// </summary>
public interface IGameSyncService
{
    /// <summary>
    /// Sincroniza todos los lanzamientos del año indicado desde la fuente externa.
    /// </summary>
    /// <param name="year">Año a sincronizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SyncAsync(int year, CancellationToken cancellationToken = default);
}
