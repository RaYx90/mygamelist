namespace GameList.Domain.Enums;

/// <summary>
/// Categoría de un juego según la clasificación de IGDB.
/// </summary>
public enum GameCategoryEnum
{
    /// <summary>Juego principal.</summary>
    MainGame = 0,

    /// <summary>DLC o complemento descargable.</summary>
    DlcAddon = 1,

    /// <summary>Expansión del juego base.</summary>
    Expansion = 2,

    /// <summary>Bundle o pack de juegos.</summary>
    Bundle = 3,

    /// <summary>Expansión independiente (no requiere el juego base).</summary>
    StandaloneExpansion = 4,

    /// <summary>Modificación creada por la comunidad.</summary>
    Mod = 5,

    /// <summary>Episodio individual de una serie.</summary>
    Episode = 6,

    /// <summary>Temporada o pase de temporada.</summary>
    Season = 7,

    /// <summary>Remake del juego original.</summary>
    Remake = 8,

    /// <summary>Remasterización del juego original.</summary>
    Remaster = 9,

    /// <summary>Juego ampliado respecto a su versión original.</summary>
    ExpandedGame = 10,

    /// <summary>Adaptación a otra plataforma.</summary>
    Port = 11,

    /// <summary>Fork o bifurcación del juego original.</summary>
    Fork = 12,

    /// <summary>Pack de contenido adicional.</summary>
    Pack = 13,

    /// <summary>Actualización del juego.</summary>
    Update = 14,

    /// <summary>Categoría desconocida o no mapeada.</summary>
    Unknown = 99
}
