namespace GameList.Application.Features.Platforms.DTOs;

/// <summary>
/// DTO con los datos de una plataforma de videojuegos.
/// </summary>
/// <param name="Id">Identificador interno de la plataforma.</param>
/// <param name="Name">Nombre de la plataforma.</param>
/// <param name="Slug">Slug único de la plataforma.</param>
/// <param name="Abbreviation">Abreviatura de la plataforma (opcional).</param>
public sealed record PlatformDto(
    int Id,
    string Name,
    string Slug,
    string? Abbreviation
);
