namespace GameList.Infrastructure.Auth;

public sealed class JwtOptionsConfig
{
    public const string SectionName = "Jwt";
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryDays { get; init; } = 30;
}
