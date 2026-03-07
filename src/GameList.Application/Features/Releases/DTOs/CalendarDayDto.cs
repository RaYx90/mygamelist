namespace GameList.Application.Features.Releases.DTOs;

public sealed record CalendarDayDto(
    DateOnly Date,
    IReadOnlyList<GameReleaseDto> Releases
);
