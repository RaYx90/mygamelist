namespace GameList.Infrastructure.Clients.LibreTranslate;

public sealed class LibreTranslateOptionsConfig
{
    public const string SectionName = "LibreTranslate";

    public string ApiUrl { get; init; } = "http://localhost:5000";
}
