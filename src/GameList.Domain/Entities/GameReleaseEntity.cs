using GameList.Domain.Enums;

namespace GameList.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa el lanzamiento de un juego en una plataforma concreta.
/// </summary>
public sealed class GameReleaseEntity
{
    /// <summary>Identificador interno del lanzamiento.</summary>
    public int Id { get; private set; }

    /// <summary>Identificador del juego al que pertenece el lanzamiento.</summary>
    public int GameId { get; private set; }

    /// <summary>Identificador de la plataforma en la que se lanza el juego.</summary>
    public int PlatformId { get; private set; }

    /// <summary>Fecha de lanzamiento del juego en la plataforma.</summary>
    public DateOnly ReleaseDate { get; private set; }

    /// <summary>Región geográfica del lanzamiento (opcional).</summary>
    public string? Region { get; private set; }

    /// <summary>Tipo de lanzamiento: exclusivo o multiplataforma.</summary>
    public ReleaseTypeEnum ReleaseType { get; private set; }

    /// <summary>Juego asociado al lanzamiento.</summary>
    public GameEntity? Game { get; private set; }

    /// <summary>Plataforma asociada al lanzamiento.</summary>
    public PlatformEntity? Platform { get; private set; }

    private GameReleaseEntity() { }

    /// <summary>
    /// Crea una nueva instancia de <see cref="GameReleaseEntity"/>.
    /// </summary>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="platformId">Identificador de la plataforma.</param>
    /// <param name="releaseDate">Fecha de lanzamiento.</param>
    /// <param name="releaseType">Tipo de lanzamiento (exclusivo o multiplataforma).</param>
    /// <param name="region">Región del lanzamiento (opcional).</param>
    /// <returns>Nueva instancia de <see cref="GameReleaseEntity"/>.</returns>
    public static GameReleaseEntity Create(
        int gameId,
        int platformId,
        DateOnly releaseDate,
        ReleaseTypeEnum releaseType,
        string? region = null)
    {
        if (gameId <= 0)
            throw new ArgumentException("El GameId debe ser un número positivo.", nameof(gameId));

        if (platformId <= 0)
            throw new ArgumentException("El PlatformId debe ser un número positivo.", nameof(platformId));

        return new GameReleaseEntity
        {
            GameId = gameId,
            PlatformId = platformId,
            ReleaseDate = releaseDate,
            ReleaseType = releaseType,
            Region = region?.Trim()
        };
    }

    /// <summary>
    /// Actualiza el tipo de lanzamiento del juego.
    /// </summary>
    /// <param name="releaseType">Nuevo tipo de lanzamiento.</param>
    public void UpdateReleaseType(ReleaseTypeEnum releaseType)
    {
        ReleaseType = releaseType;
    }
}
