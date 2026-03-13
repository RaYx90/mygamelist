namespace GameList.Domain.Entities;

/// <summary>
/// Entidad de dominio que representa una plataforma de videojuegos (consola, PC, etc.).
/// </summary>
public sealed class PlatformEntity
{
    /// <summary>Identificador interno de la plataforma.</summary>
    public int Id { get; private set; }

    /// <summary>Nombre de la plataforma.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Slug único de la plataforma (identificador legible en URL).</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Identificador de la plataforma en la API de IGDB.</summary>
    public long IgdbId { get; private set; }

    /// <summary>Abreviatura de la plataforma (opcional), p. ej. "PS5" o "XSX".</summary>
    public string? Abbreviation { get; private set; }

    private PlatformEntity() { }

    /// <summary>
    /// Crea una nueva instancia de <see cref="PlatformEntity"/>.
    /// </summary>
    /// <param name="name">Nombre de la plataforma.</param>
    /// <param name="slug">Slug único de la plataforma.</param>
    /// <param name="igdbId">Identificador en IGDB.</param>
    /// <param name="abbreviation">Abreviatura de la plataforma (opcional).</param>
    /// <returns>Nueva instancia de <see cref="PlatformEntity"/>.</returns>
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

    /// <summary>
    /// Actualiza la abreviatura de la plataforma.
    /// </summary>
    /// <param name="abbreviation">Nueva abreviatura (puede ser nula).</param>
    public void UpdateAbbreviation(string? abbreviation)
    {
        Abbreviation = abbreviation?.Trim();
    }
}
