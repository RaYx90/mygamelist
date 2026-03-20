namespace GameList.Application.Features.Social.DTOs;

/// <summary>
/// DTO con el resumen mínimo de un juego para listas de favoritos y compras en el contexto social.
/// </summary>
/// <param name="GameId">Identificador interno del juego.</param>
/// <param name="GameName">Nombre del juego.</param>
/// <param name="CoverImageUrl">URL de la portada del juego (opcional).</param>
/// <param name="ReleaseDate">Fecha de lanzamiento más temprana del juego (opcional).</param>
public sealed record GameSummaryDto(
    int GameId,
    string GameName,
    string? CoverImageUrl,
    DateOnly? ReleaseDate
);
