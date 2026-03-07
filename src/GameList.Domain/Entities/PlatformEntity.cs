namespace GameList.Domain.Entities;

public sealed class PlatformEntity
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public long IgdbId { get; private set; }
    public string? Abbreviation { get; private set; }

    private PlatformEntity() { }

    public static PlatformEntity Create(
        string name,
        string slug,
        long igdbId,
        string? abbreviation = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Platform name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Platform slug cannot be empty.", nameof(slug));

        if (igdbId <= 0)
            throw new ArgumentException("IgdbId must be a positive number.", nameof(igdbId));

        return new PlatformEntity
        {
            Name = name.Trim(),
            Slug = slug.Trim(),
            IgdbId = igdbId,
            Abbreviation = abbreviation?.Trim()
        };
    }

    public void UpdateAbbreviation(string? abbreviation)
    {
        Abbreviation = abbreviation?.Trim();
    }
}
