namespace GameList.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un juego marcado como favorito por un usuario.
/// </summary>
public sealed class GameFavoriteEntity
{
    /// <summary>Identificador interno del favorito.</summary>
    public int Id { get; private set; }

    /// <summary>Identificador del usuario que marcó el juego como favorito.</summary>
    public int UserId { get; private set; }

    /// <summary>Identificador del juego marcado como favorito.</summary>
    public int GameId { get; private set; }

    /// <summary>Fecha en que se añadió el juego a favoritos (UTC).</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Usuario que marcó el favorito.</summary>
    public UserEntity? User { get; private set; }

    /// <summary>Juego marcado como favorito.</summary>
    public GameEntity? Game { get; private set; }

    private GameFavoriteEntity() { }

    /// <summary>
    /// Crea una nueva instancia de <see cref="GameFavoriteEntity"/> para el usuario y juego indicados.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <returns>Nueva instancia de <see cref="GameFavoriteEntity"/>.</returns>
    public static GameFavoriteEntity Create(int userId, int gameId) =>
        new() { UserId = userId, GameId = gameId, CreatedAt = DateTime.UtcNow };
}
