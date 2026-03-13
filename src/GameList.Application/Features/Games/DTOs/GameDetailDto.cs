using GameList.Application.Features.Releases.DTOs;

namespace GameList.Application.Features.Games.DTOs;

/// <summary>
/// DTO con el detalle completo de un juego, incluyendo sus lanzamientos por plataforma.
/// </summary>
/// <param name="Id">Identificador interno del juego.</param>
/// <param name="Name">Nombre del juego.</param>
/// <param name="Slug">Slug único del juego.</param>
/// <param name="Summary">Descripción del juego en inglés (opcional).</param>
/// <param name="SummaryEs">Descripción del juego en español (opcional).</param>
/// <param name="CoverImageUrl">URL de la imagen de portada (opcional).</param>
/// <param name="Releases">Lista de lanzamientos del juego por plataforma.</param>
public sealed record GameDetailDto(
    int Id,
    string Name,
    string Slug,
    string? Summary,
    string? SummaryEs,
    string? CoverImageUrl,
    IReadOnlyList<GameReleaseDto> Releases
);
