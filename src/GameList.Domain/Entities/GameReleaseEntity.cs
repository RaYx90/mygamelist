using GameList.Domain.Enums;

namespace GameList.Domain.Entities;

public sealed class GameReleaseEntity
{
    public int Id { get; private set; }
    public int GameId { get; private set; }
    public int PlatformId { get; private set; }
    public DateOnly ReleaseDate { get; private set; }
    public string? Region { get; private set; }
    public ReleaseTypeEnum ReleaseType { get; private set; }

    public GameEntity? Game { get; private set; }
    public PlatformEntity? Platform { get; private set; }

    private GameReleaseEntity() { }

    public static GameReleaseEntity Create(
        int gameId,
        int platformId,
        DateOnly releaseDate,
        ReleaseTypeEnum releaseType,
        string? region = null)
    {
        if (gameId <= 0)
            throw new ArgumentException("GameId must be a positive number.", nameof(gameId));

        if (platformId <= 0)
            throw new ArgumentException("PlatformId must be a positive number.", nameof(platformId));

        return new GameReleaseEntity
        {
            GameId = gameId,
            PlatformId = platformId,
            ReleaseDate = releaseDate,
            ReleaseType = releaseType,
            Region = region?.Trim()
        };
    }

    public void UpdateReleaseType(ReleaseTypeEnum releaseType)
    {
        ReleaseType = releaseType;
    }
}
