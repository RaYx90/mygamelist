namespace GameList.Domain.Entities;

public sealed class GameFavoriteEntity
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public int GameId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public UserEntity? User { get; private set; }
    public GameEntity? Game { get; private set; }

    private GameFavoriteEntity() { }

    public static GameFavoriteEntity Create(int userId, int gameId) =>
        new() { UserId = userId, GameId = gameId, CreatedAt = DateTime.UtcNow };
}
