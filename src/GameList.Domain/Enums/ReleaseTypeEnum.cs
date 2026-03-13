namespace GameList.Domain.Enums;

/// <summary>
/// Indica si un juego es exclusivo de una plataforma o está disponible en varias.
/// </summary>
public enum ReleaseTypeEnum
{
    /// <summary>El juego se lanza únicamente en una plataforma.</summary>
    Exclusive = 1,

    /// <summary>El juego se lanza en varias plataformas.</summary>
    Multiplatform = 2
}
