using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Enums;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

/// <summary>
/// Consulta MediatR para obtener los lanzamientos de un mes agrupados por día del calendario.
/// </summary>
/// <param name="Year">Año del calendario.</param>
/// <param name="Month">Mes del calendario (1-12).</param>
/// <param name="PlatformId">Filtro por plataforma (opcional).</param>
/// <param name="Category">Filtro por categoría de juego (opcional).</param>
/// <param name="IsIndie">Filtro para incluir o excluir juegos indie (opcional).</param>
public sealed record GetReleasesByMonthQuery(
    int Year,
    int Month,
    int? PlatformId = null,
    GameCategoryEnum? Category = null,
    bool? IsIndie = null
) : IRequest<IReadOnlyList<CalendarDayDto>>;
