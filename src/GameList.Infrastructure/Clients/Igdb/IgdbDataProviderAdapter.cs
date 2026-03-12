using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GameList.Domain.Enums;
using GameList.Domain.Ports;
using Microsoft.Extensions.Options;

namespace GameList.Infrastructure.Clients.Igdb;

internal sealed class IgdbDataProviderAdapter : IGameDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly IgdbTokenService _tokenService;
    private readonly IgdbOptionsConfig _options;

    // IGDB region codes: 1=Europe, 8=Worldwide (included to capture multiregion releases)
    private const string EuropeanRegions = "1,8";

    // Current-gen platforms only: PC, Nintendo Switch, Nintendo Switch 2, PS5, Xbox Series X|S
    private const string AllowedPlatforms = "6,130,167,169,508";

    private const string ReleaseDateFields =
        "fields id,date,human,region,platform.id,platform.name,platform.slug,platform.abbreviation," +
        "game.id,game.name,game.slug,game.summary,game.cover.url,game.category,game.themes.id; ";

    public IgdbDataProviderAdapter(
        HttpClient httpClient,
        IgdbTokenService tokenService,
        IOptions<IgdbOptionsConfig> options)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<GameReleaseData>> GetReleasesForYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        var startUnix = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(year, 12, 31, 23, 59, 59, TimeSpan.Zero).ToUnixTimeSeconds();

        var results = new List<IgdbReleaseDateModel>();
        const int pageSize = 500;
        int offset = 0;

        var token = await _tokenService.GetAccessTokenAsync(cancellationToken);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Client-ID", _options.ClientId);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        while (true)
        {
            var query =
                ReleaseDateFields +
                $"where date >= {startUnix} & date <= {endUnix}" +
                $" & platform = ({AllowedPlatforms})" +
                $" & game != null & platform != null" +
                $" & (game.themes = null | game.themes != (38));" +
                $" limit {pageSize}; offset {offset};";

            var response = await _httpClient.PostAsync(
                $"{_options.BaseUrl}/release_dates",
                new StringContent(query, Encoding.UTF8, "text/plain"),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var page = await response.Content
                .ReadFromJsonAsync<List<IgdbReleaseDateModel>>(cancellationToken: cancellationToken)
                ?? [];

            results.AddRange(page);

            if (page.Count < pageSize) break;
            offset += pageSize;
        }

        return results
            .Where(r => r.Date.HasValue && r.Game is not null && r.Platform is not null)
            .DistinctBy(r => (r.Game!.Id, r.Platform!.Id))
            .Select(r => new GameReleaseData(
                IgdbGameId: r.Game!.Id,
                GameName: r.Game.Name,
                GameSlug: r.Game.Slug,
                Summary: r.Game.Summary,
                CoverImageUrl: NormalizeCoverUrl(r.Game.Cover?.Url),
                IgdbPlatformId: r.Platform!.Id,
                PlatformName: r.Platform.Name,
                PlatformSlug: r.Platform.Slug,
                PlatformAbbreviation: r.Platform.Abbreviation,
                ReleaseDate: DateTimeOffset.FromUnixTimeSeconds(r.Date!.Value).UtcDateTime.ToDateOnly(),
                Region: MapRegion(r.Region),
                GameCategory: MapCategory(r.Game!.Category),
                IsIndie: r.Game!.Themes?.Any(t => t.Id == 38) == true
            ))
            .ToList()
            .AsReadOnly();
    }

    private static string? NormalizeCoverUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        // IGDB returns protocol-relative URLs like //images.igdb.com/...
        if (url.StartsWith("//")) return "https:" + url;
        return url;
    }

    private static GameCategoryEnum MapCategory(int? category) =>
        category is int c && Enum.IsDefined(typeof(GameCategoryEnum), c)
            ? (GameCategoryEnum)c
            : GameCategoryEnum.Unknown;

    private static string? MapRegion(int? regionCode) => regionCode switch
    {
        1 => "Europe",
        2 => "North America",
        3 => "Australia",
        4 => "New Zealand",
        5 => "Japan",
        6 => "China",
        7 => "Asia",
        8 => "Worldwide",
        _ => null
    };
}

internal static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day);
}
