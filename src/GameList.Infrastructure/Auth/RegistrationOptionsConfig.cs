namespace GameList.Infrastructure.Auth;

/// <summary>
/// Opciones de configuración para el registro de nuevos usuarios.
/// Se vincula a la sección "Registration" del archivo de configuración.
/// </summary>
public sealed class RegistrationOptionsConfig
{
    /// <summary>Nombre de la sección en appsettings.json.</summary>
    public const string SectionName = "Registration";

    /// <summary>Código secreto requerido para poder registrarse en la aplicación.</summary>
    public string SecretCode { get; init; } = string.Empty;
}
