using GameList.Application.Features.Releases.DTOs;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

/// <summary>
/// Busca lanzamientos por nombre a lo largo de todo un año.
/// </summary>
/// <param name="Year">Año en el que buscar.</param>
/// <param name="Name">Texto parcial del nombre del juego.</param>
public sealed record SearchReleasesQuery(int Year, string Name) : IRequest<IReadOnlyList<CalendarDayDto>>;
