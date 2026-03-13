using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using GameList.Domain.Enums;
using GameList.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace GameList.Infrastructure.Clients.Igdb;

/// <summary>
/// Adaptador para la API de IGDB (Twitch). Obtiene lanzamientos de juegos usando
/// el lenguaje de consulta Apicalypse de IGDB con paginación automática.
/// </summary>
/// <remarks>
/// Plataformas soportadas (AllowedPlatforms): PC(6), Switch(130), Switch 2(167), PS5(169), Xbox Series X|S(508).
/// Juegos indie excluidos con <c>game.themes != (38)</c> donde 38 es el tema "Indie" en IGDB.
/// Paginación en bloques de 500 (máximo permitido por IGDB por request).
/// </remarks>
internal sealed class IgdbDataProviderAdapter : IGameDataProvider
{
    private readonly HttpClient httpClient;
    private readonly IgdbTokenService tokenService;
    private readonly IgdbOptionsConfig options;

    // Códigos de región de IGDB: 1=Europa, 8=Worldwide. Definido pero no usado actualmente
    // en la query — el filtro de región fue descartado porque IGDB no asigna región a muchos juegos de 2026.
    private const string EuropeanRegions = "1,8";

    // IDs de plataformas de generación actual en IGDB.
    private const string AllowedPlatforms = "6,130,167,169,508";

    // Campos que se solicitan en la query Apicalypse. Se incluyen datos del juego, plataforma y temas.
    private const string ReleaseDateFields =
        "fields id,date,human,region,platform.id,platform.name,platform.slug,platform.abbreviation," +
        "game.id,game.name,game.slug,game.summary,game.cover.url,game.category,game.themes.id; ";

    public IgdbDataProviderAdapter(
        HttpClient httpClient,
        IgdbTokenService tokenService,
        IOptions<IgdbOptionsConfig> options)
    {
        this.httpClient = httpClient;
        this.tokenService = tokenService;
        this.options = options.Value;
    }

    /// <summary>
    /// Obtiene todos los lanzamientos de juegos para el año indicado desde IGDB,
    /// paginando automáticamente en bloques de 500 hasta agotar los resultados.
    /// </summary>
    public async Task<IReadOnlyList<GameReleaseData>> GetReleasesForYearAsync(
        int year,
        CancellationToken cancellationToken = default)
    {
        // IGDB usa timestamps Unix para filtrar por fecha.
        var startUnix = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        var endUnix = new DateTimeOffset(year, 12, 31, 23, 59, 59, TimeSpan.Zero).ToUnixTimeSeconds();

        var results = new List<IgdbReleaseDateModel>();
        const int pageSize = 500; // Máximo permitido por IGDB por request.
        int offset = 0;

        // Se obtiene el token OAuth de Twitch (se cachea internamente en IgdbTokenService).
        var token = await tokenService.GetAccessTokenAsync(cancellationToken);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders.Add("Client-ID", options.ClientId);
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        // Bucle de paginación: continúa hasta que una página devuelva menos resultados que el tamaño de página.
        while (true)
        {
            // Query en lenguaje Apicalypse (formato propietario de IGDB):
            // - Filtra por rango de fecha Unix
            // - Solo plataformas de generación actual (AllowedPlatforms)
            // - Excluye juegos con tema 38 (Indie) o juegos sin tema asignado que sean indie
            // - La condición (game.themes = null | game.themes != (38)) incluye juegos sin temas
            //   y excluye los que tienen el tema Indie explícitamente.
            var query =
                ReleaseDateFields +
                $"where date >= {startUnix} & date <= {endUnix}" +
                $" & platform = ({AllowedPlatforms})" +
                $" & game != null & platform != null" +
                $" & (game.themes = null | game.themes != (38));" +
                $" limit {pageSize}; offset {offset};";

            var response = await httpClient.PostAsync(
                $"{options.BaseUrl}/release_dates",
                new StringContent(query, Encoding.UTF8, "text/plain"),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var page = await response.Content
                .ReadFromJsonAsync<List<IgdbReleaseDateModel>>(cancellationToken: cancellationToken)
                ?? [];

            results.AddRange(page);

            // Si la página tiene menos registros que el tamaño máximo, es la última página.
            if (page.Count < pageSize) break;
            offset += pageSize;
        }

        return results
            .Where(r => r.Date.HasValue && r.Game is not null && r.Platform is not null)
            // Elimina duplicados del mismo juego+plataforma (puede haber varias fechas de lanzamiento).
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
                // Se mantiene el campo IsIndie en el modelo aunque no se use en la UI actualmente.
                IsIndie: r.Game!.Themes?.Any(t => t.Id == 38) == true
            ))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// IGDB devuelve URLs relativas al protocolo (//images.igdb.com/...).
    /// Se les añade "https:" para que sean URLs absolutas válidas.
    /// </summary>
    private static string? NormalizeCoverUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        if (url.StartsWith("//")) return "https:" + url;
        return url;
    }

    /// <summary>Convierte el entero de categoría de IGDB al enum interno. Devuelve Unknown si no está definido.</summary>
    private static GameCategoryEnum MapCategory(int? category) =>
        category is int c && Enum.IsDefined(typeof(GameCategoryEnum), c)
            ? (GameCategoryEnum)c
            : GameCategoryEnum.Unknown;

    /// <summary>Convierte el código de región numérico de IGDB a nombre legible.</summary>
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

/// <summary>Extensión de DateTime para convertir a DateOnly sin dependencia de Polly ni BCL adicional.</summary>
internal static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTime dt) =>
        new(dt.Year, dt.Month, dt.Day);
}
