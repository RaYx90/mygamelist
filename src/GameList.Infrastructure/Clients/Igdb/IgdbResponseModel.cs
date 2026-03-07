using System.Text.Json.Serialization;

namespace GameList.Infrastructure.Clients.Igdb;

internal sealed record IgdbReleaseDateModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("date")] long? Date,
    [property: JsonPropertyName("human")] string? Human,
    [property: JsonPropertyName("region")] int? Region,
    [property: JsonPropertyName("platform")] IgdbPlatformModel? Platform,
    [property: JsonPropertyName("game")] IgdbGameModel? Game
);

internal sealed record IgdbGameModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("cover")] IgdbCoverModel? Cover,
    [property: JsonPropertyName("category")] int? Category,
    [property: JsonPropertyName("themes")] IReadOnlyList<IgdbThemeModel>? Themes
);

internal sealed record IgdbPlatformModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("abbreviation")] string? Abbreviation
);

internal sealed record IgdbCoverModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("url")] string? Url
);

internal sealed record IgdbThemeModel(
    [property: JsonPropertyName("id")] int Id
);
