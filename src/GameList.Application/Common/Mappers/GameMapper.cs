using GameList.Application.Features.Games.DTOs;
using GameList.Application.Features.Platforms.DTOs;
using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Entities;
using GameList.Domain.Enums;

namespace GameList.Application.Common.Mappers;

public static class GameMapper
{
    public static GameReleaseDto ToDto(GameReleaseEntity release)
    {
        var label = release.Platform?.Abbreviation ?? release.Platform?.Name ?? string.Empty;
        return new(
            Id: release.Id,
            GameId: release.GameId,
            GameName: release.Game?.Name ?? string.Empty,
            GameSlug: release.Game?.Slug ?? string.Empty,
            Summary: release.Game?.Summary,
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

    public static PlatformDto ToPlatformDto(PlatformEntity platform) => new(
        Id: platform.Id,
        Name: platform.Name,
        Slug: platform.Slug,
        Abbreviation: platform.Abbreviation
    );

    public static GameDetailDto ToDetailDto(GameEntity game) => new(
        Id: game.Id,
        Name: game.Name,
        Slug: game.Slug,
        Summary: game.Summary,
        CoverImageUrl: game.CoverImageUrl,
        Releases: game.Releases.Select(ToDto).ToList().AsReadOnly()
    );
}
