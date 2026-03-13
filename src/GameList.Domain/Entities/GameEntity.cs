using GameList.Domain.Enums;
using GameList.Domain.Exceptions;

namespace GameList.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa un videojuego.
/// </summary>
public sealed class GameEntity
{
    /// <summary>Identificador interno del juego.</summary>
    public int Id { get; private set; }

    /// <summary>Nombre del juego.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Descripción original del juego en inglés.</summary>
    public string? Summary { get; private set; }

    /// <summary>Descripción traducida al español.</summary>
    public string? SummaryEs { get; private set; }

    /// <summary>URL de la imagen de portada.</summary>
    public string? CoverImageUrl { get; private set; }

    /// <summary>Slug único del juego (identificador legible en URL).</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Identificador del juego en la API de IGDB.</summary>
    public long IgdbId { get; private set; }

    /// <summary>Categoría del juego según IGDB (juego principal, DLC, expansión, etc.).</summary>
    public GameCategoryEnum Category { get; private set; }

    /// <summary>Indica si el juego es indie.</summary>
    public bool IsIndie { get; private set; }

    /// <summary>Fecha de creación del registro en UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Fecha de última modificación del registro en UTC.</summary>
    public DateTime UpdatedAt { get; private set; }

    private readonly List<GameReleaseEntity> releases = [];

    /// <summary>Colección de lanzamientos asociados al juego.</summary>
    public IReadOnlyCollection<GameReleaseEntity> Releases => releases.AsReadOnly();

    private GameEntity() { }

    /// <summary>
    /// Crea una nueva instancia de <see cref="GameEntity"/> con los datos proporcionados.
    /// </summary>
    /// <param name="name">Nombre del juego.</param>
    /// <param name="slug">Slug único del juego.</param>
    /// <param name="igdbId">Identificador en IGDB.</param>
    /// <param name="summary">Descripción en inglés (opcional).</param>
    /// <param name="coverImageUrl">URL de la portada (opcional).</param>
    /// <param name="category">Categoría del juego.</param>
    /// <param name="isIndie">Indica si es un juego indie.</param>
    /// <returns>Nueva instancia de <see cref="GameEntity"/>.</returns>
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
            throw new ArgumentException("El nombre del juego no puede estar vacío.", nameof(name));

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("El slug del juego no puede estar vacío.", nameof(slug));

        if (igdbId <= 0)
            throw new ArgumentException("El IgdbId debe ser un número positivo.", nameof(igdbId));

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

    /// <summary>
    /// Actualiza la descripción traducida al español del juego.
    /// </summary>
    /// <param name="summaryEs">Texto traducido al español.</param>
    public void UpdateSummaryEs(string? summaryEs)
    {
        SummaryEs = summaryEs?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza los datos principales del juego.
    /// </summary>
    /// <param name="name">Nuevo nombre del juego.</param>
    /// <param name="summary">Nueva descripción en inglés.</param>
    /// <param name="coverImageUrl">Nueva URL de portada.</param>
    /// <param name="category">Nueva categoría del juego.</param>
    /// <param name="isIndie">Indica si es indie.</param>
    public void Update(
        string name,
        string? summary,
        string? coverImageUrl,
        GameCategoryEnum category,
        bool isIndie = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del juego no puede estar vacío.", nameof(name));

        Name = name.Trim();
        Summary = summary?.Trim();
        CoverImageUrl = coverImageUrl?.Trim();
        Category = category;
        IsIndie = isIndie;
        UpdatedAt = DateTime.UtcNow;
    }
}
