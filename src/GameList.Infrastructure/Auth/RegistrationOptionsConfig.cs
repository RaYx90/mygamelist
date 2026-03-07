namespace GameList.Infrastructure.Auth;

public sealed class RegistrationOptionsConfig
{
    public const string SectionName = "Registration";
    public string SecretCode { get; init; } = string.Empty;
}
