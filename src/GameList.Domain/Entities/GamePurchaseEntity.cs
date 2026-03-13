namespace GameList.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un juego marcado como comprado por un usuario.
/// </summary>
public sealed class GamePurchaseEntity
{
    /// <summary>Identificador interno del registro de compra.</summary>
    public int Id { get; private set; }

    /// <summary>Identificador del usuario que marcó el juego como comprado.</summary>
    public int UserId { get; private set; }

    /// <summary>Identificador del juego marcado como comprado.</summary>
    public int GameId { get; private set; }

    /// <summary>Fecha en que se registró la compra (UTC).</summary>
    public DateTime PurchasedAt { get; private set; }

    /// <summary>Usuario que registró la compra.</summary>
    public UserEntity? User { get; private set; }

    /// <summary>Juego marcado como comprado.</summary>
    public GameEntity? Game { get; private set; }

    private GamePurchaseEntity() { }

    /// <summary>
    /// Crea una nueva instancia de <see cref="GamePurchaseEntity"/> para el usuario y juego indicados.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <returns>Nueva instancia de <see cref="GamePurchaseEntity"/>.</returns>
    public static GamePurchaseEntity Create(int userId, int gameId) =>
        new() { UserId = userId, GameId = gameId, PurchasedAt = DateTime.UtcNow };
}
