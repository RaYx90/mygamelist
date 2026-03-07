namespace GameList.Infrastructure.Clients.Igdb;

public sealed class IgdbOptionsConfig
{
    public const string SectionName = "Igdb";

    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string TokenUrl { get; init; } = "https://id.twitch.tv/oauth2/token";
    public string BaseUrl { get; init; } = "https://api.igdb.com/v4";
}
