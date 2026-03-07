using GameList.Domain.Ports;

namespace GameList.Api.Tests.Common;

/// <summary>
/// Returns a fixed set of game release data for testing, without hitting the IGDB API.
/// </summary>
public sealed class FakeGameDataProvider : IGameDataProvider
{
    public static readonly IReadOnlyList<GameReleaseData> SampleData =
    [
        new(IgdbGameId: 1001, GameName: "Test Game Alpha", GameSlug: "test-game-alpha",
            Summary: "A test action game.", CoverImageUrl: null,
            IgdbPlatformId: 501, PlatformName: "PlayStation 5", PlatformSlug: "ps5",
            PlatformAbbreviation: "PS5",
            ReleaseDate: new DateOnly(DateTime.UtcNow.Year, 3, 15), Region: "Worldwide"),

        new(IgdbGameId: 1002, GameName: "Test Game Beta", GameSlug: "test-game-beta",
            Summary: "A test RPG.", CoverImageUrl: null,
            IgdbPlatformId: 502, PlatformName: "Xbox Series X", PlatformSlug: "xbox-series-x",
            PlatformAbbreviation: "XSX",
            ReleaseDate: new DateOnly(DateTime.UtcNow.Year, 3, 20), Region: "Worldwide"),

        // Same game on two platforms → multiplatform
        new(IgdbGameId: 1003, GameName: "Multi Platform Game", GameSlug: "multi-platform-game",
            Summary: "Available everywhere.", CoverImageUrl: null,
            IgdbPlatformId: 501, PlatformName: "PlayStation 5", PlatformSlug: "ps5",
            PlatformAbbreviation: "PS5",
            ReleaseDate: new DateOnly(DateTime.UtcNow.Year, 3, 25), Region: "Worldwide"),

        new(IgdbGameId: 1003, GameName: "Multi Platform Game", GameSlug: "multi-platform-game",
            Summary: "Available everywhere.", CoverImageUrl: null,
            IgdbPlatformId: 502, PlatformName: "Xbox Series X", PlatformSlug: "xbox-series-x",
            PlatformAbbreviation: "XSX",
            ReleaseDate: new DateOnly(DateTime.UtcNow.Year, 3, 25), Region: "Worldwide"),
    ];

    public Task<IReadOnlyList<GameReleaseData>> GetReleasesForYearAsync(
        int year,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<GameReleaseData>>(
            SampleData.Where(d => d.ReleaseDate.Year == year).ToList().AsReadOnly());
}
