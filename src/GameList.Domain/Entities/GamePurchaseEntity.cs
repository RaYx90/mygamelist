namespace GameList.Domain.Entities;

public sealed class GamePurchaseEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int GameId { get; private set; }
    public DateTime PurchasedAt { get; private set; }

    public UserEntity? User { get; private set; }
    public GameEntity? Game { get; private set; }

    private GamePurchaseEntity() { }

    public static GamePurchaseEntity Create(int userId, int gameId) =>
        new() { UserId = userId, GameId = gameId, PurchasedAt = DateTime.UtcNow };
}
