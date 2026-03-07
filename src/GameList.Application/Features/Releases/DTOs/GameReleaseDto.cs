using GameList.Domain.Enums;

namespace GameList.Application.Features.Releases.DTOs;

public sealed record GameReleaseDto(
    int Id,
    int GameId,
    string GameName,
    string GameSlug,
    string? Summary,
    string? SummaryEs,
    string? CoverImageUrl,
    int PlatformId,
    string PlatformName,
    string? PlatformAbbreviation,
    DateOnly ReleaseDate,
    string? Region,
    ReleaseTypeEnum ReleaseType,
    IReadOnlyList<string> AllPlatformLabels,
    GameCategoryEnum GameCategory = GameCategoryEnum.MainGame,
    bool IsIndie = false
);
