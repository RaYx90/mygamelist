using GameList.Domain.Enums;
using GameList.Domain.Exceptions;

namespace GameList.Domain.Entities;

public sealed class GameEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Summary { get; private set; }
    public string? SummaryEs { get; private set; }
    public string? CoverImageUrl { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public long IgdbId { get; private set; }
    public GameCategoryEnum Category { get; private set; }
    public bool IsIndie { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private readonly List<GameReleaseEntity> _releases = [];
    public IReadOnlyCollection<GameReleaseEntity> Releases => _releases.AsReadOnly();

    private GameEntity() { }

    public static GameEntity Create(
        string name,
        string slug,
        long igdbId,
        string? summary = null,
        string? coverImageUrl = null,
        GameCategoryEnum category = GameCategoryEnum.MainGame,
        bool isIndie = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Game name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Game slug cannot be empty.", nameof(slug));

        if (igdbId <= 0)
            throw new ArgumentException("IgdbId must be a positive number.", nameof(igdbId));

        var now = DateTime.UtcNow;
        return new GameEntity
        {
            Name = name.Trim(),
            Slug = slug.Trim(),
            IgdbId = igdbId,
            Summary = summary?.Trim(),
            CoverImageUrl = coverImageUrl?.Trim(),
            Category = category,
            IsIndie = isIndie,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public void UpdateSummaryEs(string? summaryEs)
    {
        SummaryEs = summaryEs?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(
        string name,
        string? summary,
        string? coverImageUrl,
        GameCategoryEnum category,
        bool isIndie = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Game name cannot be empty.", nameof(name));

        Name = name.Trim();
        Summary = summary?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        Category = category;
        IsIndie = isIndie;
        UpdatedAt = DateTime.UtcNow;
    }
}
