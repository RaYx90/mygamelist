using GameList.Domain.Enums;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Interfaces;

/// <summary>
/// Puerto de dominio para obtener datos de lanzamientos desde una fuente externa (p. ej. IGDB).
/// </summary>
public interface IGameDataProvider
{
    /// <summary>
    /// Obtiene todos los lanzamientos de videojuegos para el año indicado desde la fuente externa.
    /// </summary>
    /// <param name="year">Año del que se obtienen los lanzamientos.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de datos de lanzamientos del año especificado.</returns>
    Task<IReadOnlyList<GameReleaseData>> GetReleasesForYearAsync(
        int year,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Datos de un lanzamiento de juego obtenidos de la fuente externa.
/// </summary>
/// <param name="IgdbGameId">Identificador del juego en IGDB.</param>
/// <param name="GameName">Nombre del juego.</param>
/// <param name="GameSlug">Slug del juego.</param>
/// <param name="Summary">Descripción del juego en inglés (opcional).</param>
/// <param name="CoverImageUrl">URL de la portada (opcional).</param>
/// <param name="IgdbPlatformId">Identificador de la plataforma en IGDB.</param>
/// <param name="PlatformName">Nombre de la plataforma.</param>
/// <param name="PlatformSlug">Slug de la plataforma.</param>
/// <param name="PlatformAbbreviation">Abreviatura de la plataforma (opcional).</param>
/// <param name="ReleaseDate">Fecha de lanzamiento.</param>
/// <param name="Region">Región del lanzamiento (opcional).</param>
/// <param name="GameCategory">Categoría del juego.</param>
/// <param name="IsIndie">Indica si el juego es indie.</param>
public sealed record GameReleaseData(
    long IgdbGameId,
    string GameName,
    string GameSlug,
    string? Summary,
    string? CoverImageUrl,
    long IgdbPlatformId,
    string PlatformName,
    string PlatformSlug,
    string? PlatformAbbreviation,
    DateOnly ReleaseDate,
    string? Region,
    GameCategoryEnum GameCategory = GameCategoryEnum.MainGame,
    bool IsIndie = false
);
