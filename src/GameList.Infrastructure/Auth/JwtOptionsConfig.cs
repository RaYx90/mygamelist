namespace GameList.Infrastructure.Auth;

/// <summary>
/// Opciones de configuración para la generación y validación de tokens JWT.
/// Se vincula a la sección "Jwt" del archivo de configuración.
/// </summary>
public sealed class JwtOptionsConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "Jwt";

    /// <summary>Clave secreta utilizada para firmar los tokens JWT.</summary>
    public string Key { get; init; } = string.Empty;

    /// <summary>Emisor del token (claim "iss").</summary>
    public string Issuer { get; init; } = string.Empty;

    /// <summary>Audiencia del token (claim "aud").</summary>
    public string Audience { get; init; } = string.Empty;

    /// <summary>Días de validez del token desde su emisión.</summary>
    public int ExpiryDays { get; init; } = 30;
}
