using GameList.Domain.Entities;
using GameList.Domain.Enums;

namespace GameList.Domain.Services;

/// <summary>
/// Determines whether a game is exclusive to a platform or multiplatform,
/// based on the number of distinct platforms with a release within a given year.
/// A game is considered exclusive if it only has releases on a single platform.
/// </summary>
public static class ReleaseDomainService
{
    public static ReleaseTypeEnum DetermineReleaseType(
        IEnumerable<GameReleaseEntity> allReleasesForGame)
    {
        var distinctPlatforms = allReleasesForGame
            .Select(r => r.PlatformId)
            .Distinct()
            .Count();

        return distinctPlatforms == 1
            ? ReleaseTypeEnum.Exclusive
            : ReleaseTypeEnum.Multiplatform;
    }
}
