using GameList.Application.Features.Sync.Commands;
using GameList.Api.Tests.Common;
using GameList.Domain.Enums;
using GameList.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GameList.Api.Tests.Endpoints;

public sealed class SyncTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory factory;

    public SyncTests(CustomWebApplicationFactory factory) => this.factory = factory;

    [Fact]
    public async Task SyncGames_WithFakeProvider_PersistsGamesAndPlatforms()
    {
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        var result = await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));

        result.Success.Should().BeTrue();
        result.GamesProcessed.Should().Be(3); // 3 distinct games in FakeGameDataProvider
        result.PlatformsProcessed.Should().Be(2); // PS5, XSX

        var gamesInDb = await db.Games.CountAsync();
        gamesInDb.Should().Be(3);

        var platformsInDb = await db.Platforms.CountAsync();
        platformsInDb.Should().Be(2);

        var releasesInDb = await db.GameReleases.CountAsync();
        releasesInDb.Should().Be(4); // 4 release records (multi-platform game has 2)
    }

    [Fact]
    public async Task SyncGames_MultiPlatformGame_MarkedAsMultiplatform()
    {
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));

        var multiPlatformReleases = await db.GameReleases
            .Include(r => r.Game)
            .Where(r => r.Game!.Name == "Multi Platform Game")
            .ToListAsync();

        multiPlatformReleases.Should().AllSatisfy(r =>
            r.ReleaseType.Should().Be(ReleaseTypeEnum.Multiplatform));
    }

    [Fact]
    public async Task SyncGames_ExclusiveGame_MarkedAsExclusive()
    {
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));

        var exclusiveRelease = await db.GameReleases
            .Include(r => r.Game)
            .FirstAsync(r => r.Game!.Name == "Test Game Alpha");

        exclusiveRelease.ReleaseType.Should().Be(ReleaseTypeEnum.Exclusive);
    }

    [Fact]
    public async Task SyncGames_SegundoSync_NoDuplicaJuegosNiPlataformas()
    {
        // El sync usa upsert (insert-or-update), por lo que ejecutarlo dos veces
        // debe producir el mismo número de registros que ejecutarlo una sola vez.
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Primer sync
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));
        var gamesAfterFirst = await db.Games.CountAsync();
        var platformsAfterFirst = await db.Platforms.CountAsync();

        // Segundo sync — los datos de FakeGameDataProvider son idénticos
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));
        var gamesAfterSecond = await db.Games.CountAsync();
        var platformsAfterSecond = await db.Platforms.CountAsync();

        gamesAfterSecond.Should().Be(gamesAfterFirst);
        platformsAfterSecond.Should().Be(platformsAfterFirst);
    }
}
