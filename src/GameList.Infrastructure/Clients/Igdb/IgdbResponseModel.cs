using System.Text.Json.Serialization;

namespace GameList.Infrastructure.Clients.Igdb;

/// <summary>
/// Modelo de respuesta de IGDB para un registro de fecha de lanzamiento (release_date).
/// </summary>
internal sealed record IgdbReleaseDateModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("date")] long? Date,
    [property: JsonPropertyName("human")] string? Human,
    [property: JsonPropertyName("region")] int? Region,
    [property: JsonPropertyName("platform")] IgdbPlatformModel? Platform,
    [property: JsonPropertyName("game")] IgdbGameModel? Game
);

/// <summary>
/// Modelo de respuesta de IGDB para los datos de un juego.
/// </summary>
internal sealed record IgdbGameModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("summary")] string? Summary,
    [property: JsonPropertyName("cover")] IgdbCoverModel? Cover,
    [property: JsonPropertyName("category")] int? Category,
    [property: JsonPropertyName("themes")] IReadOnlyList<IgdbThemeModel>? Themes
);

/// <summary>
/// Modelo de respuesta de IGDB para los datos de una plataforma.
/// </summary>
internal sealed record IgdbPlatformModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("slug")] string Slug,
    [property: JsonPropertyName("abbreviation")] string? Abbreviation
);

/// <summary>
/// Modelo de respuesta de IGDB para la portada de un juego.
/// </summary>
internal sealed record IgdbCoverModel(
    [property: JsonPropertyName("id")] long Id,
    [property: JsonPropertyName("url")] string? Url
);

/// <summary>
/// Modelo de respuesta de IGDB para un tema asociado a un juego (usado para detectar si es indie).
/// </summary>
internal sealed record IgdbThemeModel(
    [property: JsonPropertyName("id")] int Id
);
