using MediatR;

namespace GameList.Application.Features.Sync.Commands;

/// <summary>
/// Comando MediatR para sincronizar los lanzamientos de un año desde la fuente externa (IGDB).
/// </summary>
/// <param name="Year">Año cuyos lanzamientos se sincronizarán.</param>
public sealed record SyncGamesCommand(int Year) : IRequest<SyncResultDto>;

/// <summary>
/// DTO con el resultado de la sincronización de juegos.
/// </summary>
/// <param name="Success">Indica si la sincronización completó sin errores.</param>
/// <param name="GamesProcessed">Número de juegos procesados.</param>
/// <param name="PlatformsProcessed">Número de plataformas procesadas.</param>
/// <param name="ErrorMessage">Mensaje de error si la sincronización falló (opcional).</param>
public sealed record SyncResultDto(
    bool Success,
    int GamesProcessed,
    int PlatformsProcessed,
    string? ErrorMessage = null
);
