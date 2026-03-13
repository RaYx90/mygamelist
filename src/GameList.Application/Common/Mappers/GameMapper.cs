using GameList.Application.Features.Games.DTOs;
using GameList.Application.Features.Platforms.DTOs;
using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Enums;

namespace GameList.Application.Common.Mappers;

/// <summary>
/// Mapeador estático para convertir entidades de dominio a DTOs de la capa de aplicación.
/// </summary>
public static class GameMapper
{
    /// <summary>
    /// Convierte un <see cref="GameReleaseEntity"/> a su DTO de lanzamiento.
    /// </summary>
    /// <param name="release">Entidad de lanzamiento a convertir.</param>
    /// <returns>DTO con los datos del lanzamiento.</returns>
    public static GameReleaseDto ToDto(GameReleaseEntity release)
    {
        var label = release.Platform?.Abbreviation ?? release.Platform?.Name ?? string.Empty;
        return new(
            Id: release.Id,
            GameId: release.GameId,
            GameName: release.Game?.Name ?? string.Empty,
            GameSlug: release.Game?.Slug ?? string.Empty,
            Summary: release.Game?.Summary,
            SummaryEs: release.Game?.SummaryEs,
            CoverImageUrl: release.Game?.CoverImageUrl,
            PlatformId: release.PlatformId,
            PlatformName: release.Platform?.Name ?? string.Empty,
            PlatformAbbreviation: release.Platform?.Abbreviation,
            ReleaseDate: release.ReleaseDate,
            Region: release.Region,
            ReleaseType: release.ReleaseType,
            AllPlatformLabels: [label],
            GameCategory: release.Game?.Category ?? GameCategoryEnum.MainGame,
            IsIndie: release.Game?.IsIndie ?? false
        );
    }

    /// <summary>
    /// Convierte un <see cref="PlatformEntity"/> a su DTO de plataforma.
    /// </summary>
    /// <param name="platform">Entidad de plataforma a convertir.</param>
    /// <returns>DTO con los datos de la plataforma.</returns>
    public static PlatformDto ToPlatformDto(PlatformEntity platform) => new(
        Id: platform.Id,
        Name: platform.Name,
        Slug: platform.Slug,
        Abbreviation: platform.Abbreviation
    );

    /// <summary>
    /// Convierte un <see cref="GameEntity"/> a su DTO de detalle, incluyendo sus lanzamientos.
    /// </summary>
    /// <param name="game">Entidad del juego a convertir.</param>
    /// <returns>DTO con los datos detallados del juego.</returns>
    public static GameDetailDto ToDetailDto(GameEntity game) => new(
        Id: game.Id,
        Name: game.Name,
        Slug: game.Slug,
        Summary: game.Summary,
        SummaryEs: game.SummaryEs,
        CoverImageUrl: game.CoverImageUrl,
        Releases: game.Releases.Select(ToDto).ToList().AsReadOnly()
    );
}
