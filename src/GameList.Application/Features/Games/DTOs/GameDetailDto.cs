using GameList.Application.Features.Releases.DTOs;

namespace GameList.Application.Features.Games.DTOs;

public sealed record GameDetailDto(
    int Id,
    string Name,
    string Slug,
    string? Summary,
    string? SummaryEs,
    string? CoverImageUrl,
    IReadOnlyList<GameReleaseDto> Releases
);
