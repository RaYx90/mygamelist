namespace GameList.Infrastructure.Clients.Igdb;

/// <summary>
/// Opciones de configuración para la API de IGDB (Twitch OAuth2 + endpoints).
/// Se vincula a la sección "Igdb" del archivo de configuración.
/// </summary>
public sealed class IgdbOptionsConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "Igdb";

    /// <summary>Client ID de la aplicación registrada en Twitch Developer.</summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>Client Secret de la aplicación registrada en Twitch Developer.</summary>
    public string ClientSecret { get; init; } = string.Empty;

    /// <summary>URL del endpoint de tokens OAuth2 de Twitch.</summary>
    public string TokenUrl { get; init; } = "https://id.twitch.tv/oauth2/token";

    /// <summary>URL base de la API de IGDB.</summary>
    public string BaseUrl { get; init; } = "https://api.igdb.com/v4";
}
