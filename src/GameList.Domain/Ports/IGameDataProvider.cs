using GameList.Domain.Enums;
using GameList.Domain.ValueObjects;

namespace GameList.Domain.Ports;

public interface IGameDataProvider
{
    Task<IReadOnlyList<GameReleaseData>> GetReleasesForYearAsync(
        int year,
        CancellationToken cancellationToken = default);
}

public sealed record GameReleaseData(
    long IgdbGameId,
    string GameName,
    string GameSlug,
    string? Summary,
    string? CoverImageUrl,
    long IgdbPlatformId,
    string PlatformName,
    string PlatformSlug,
    string? PlatformAbbreviation,
    DateOnly ReleaseDate,
    string? Region,
    GameCategoryEnum GameCategory = GameCategoryEnum.MainGame,
    bool IsIndie = false
);
