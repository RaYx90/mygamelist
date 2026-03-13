namespace GameList.Application.Features.Social.DTOs;

/// <summary>
/// DTO con el resumen de interés de un juego dentro de un grupo (quién lo quiere y quién lo ha comprado).
/// </summary>
/// <param name="GameId">Identificador del juego.</param>
/// <param name="GameName">Nombre del juego.</param>
/// <param name="CoverImageUrl">URL de la portada del juego (opcional).</param>
/// <param name="ReleaseDate">Fecha de lanzamiento del juego (opcional).</param>
/// <param name="WantedBy">Nombres de los miembros del grupo que tienen el juego como favorito.</param>
/// <param name="PurchasedBy">Nombres de los miembros del grupo que han comprado el juego.</param>
public sealed record GroupInsightDto(
    int GameId,
    string GameName,
    string? CoverImageUrl,
    DateOnly? ReleaseDate,
    IReadOnlyList<string> WantedBy,
    IReadOnlyList<string> PurchasedBy
);
