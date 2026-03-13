namespace GameList.Application.Features.Releases.DTOs;

/// <summary>
/// DTO que representa un día del calendario con sus lanzamientos de videojuegos.
/// </summary>
/// <param name="Date">Fecha del día del calendario.</param>
/// <param name="Releases">Lanzamientos programados para ese día.</param>
public sealed record CalendarDayDto(
    DateOnly Date,
    IReadOnlyList<GameReleaseDto> Releases
);
