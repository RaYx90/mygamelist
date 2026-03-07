namespace GameList.Infrastructure.Clients.DeepL;

public sealed class DeepLOptionsConfig
{
    public const string SectionName = "DeepL";

    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Use https://api-free.deepl.com for free-tier keys (ending in :fx),
    /// or https://api.deepl.com for paid keys.
    /// </summary>
    public string ApiUrl { get; init; } = "https://api-free.deepl.com";
}
