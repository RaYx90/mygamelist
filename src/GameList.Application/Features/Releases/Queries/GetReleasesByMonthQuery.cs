using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Enums;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

public sealed record GetReleasesByMonthQuery(
    int Year,
    int Month,
    int? PlatformId = null,
    GameCategoryEnum? Category = null,
    bool? IsIndie = null
) : IRequest<IReadOnlyList<CalendarDayDto>>;
