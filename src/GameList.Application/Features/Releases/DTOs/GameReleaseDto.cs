using GameList.Domain.Enums;

namespace GameList.Application.Features.Releases.DTOs;

/// <summary>
/// DTO con los datos de un lanzamiento de juego en una o varias plataformas para un día del calendario.
/// </summary>
/// <param name="Id">Identificador del lanzamiento.</param>
/// <param name="GameId">Identificador del juego.</param>
/// <param name="GameName">Nombre del juego.</param>
/// <param name="GameSlug">Slug del juego.</param>
/// <param name="Summary">Descripción del juego en inglés (opcional).</param>
/// <param name="SummaryEs">Descripción del juego en español (opcional).</param>
/// <param name="CoverImageUrl">URL de la portada (opcional).</param>
/// <param name="PlatformId">Identificador de la plataforma principal.</param>
/// <param name="PlatformName">Nombre de la plataforma principal.</param>
/// <param name="PlatformAbbreviation">Abreviatura de la plataforma principal (opcional).</param>
/// <param name="ReleaseDate">Fecha de lanzamiento.</param>
/// <param name="Region">Región del lanzamiento (opcional).</param>
/// <param name="ReleaseType">Tipo de lanzamiento: exclusivo o multiplataforma.</param>
/// <param name="AllPlatformLabels">Etiquetas de todas las plataformas en las que se lanza el juego ese día.</param>
/// <param name="GameCategory">Categoría del juego.</param>
/// <param name="IsIndie">Indica si el juego es indie.</param>
public sealed record GameReleaseDto(
    int Id,
    int GameId,
    string GameName,
    string GameSlug,
    string? Summary,
    string? SummaryEs,
    string? CoverImageUrl,
    int PlatformId,
    string PlatformName,
    string? PlatformAbbreviation,
    DateOnly ReleaseDate,
    string? Region,
    ReleaseTypeEnum ReleaseType,
    IReadOnlyList<string> AllPlatformLabels,
    GameCategoryEnum GameCategory = GameCategoryEnum.MainGame,
    bool IsIndie = false
);
