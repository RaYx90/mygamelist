using FluentAssertions;
using GameList.Domain.Entities;

namespace GameList.Api.Tests.Unit.Domain;

/// <summary>
/// Tests unitarios para <see cref="GameFavoriteEntity"/> y <see cref="GamePurchaseEntity"/>.
/// Verifican que los factory methods asignan correctamente UserId, GameId y el timestamp.
/// </summary>
public sealed class GameFavoriteAndPurchaseEntityTests
{
    // ── GameFavoriteEntity ────────────────────────────────────────────────────

    [Fact]
    public void GameFavorite_Create_AsignaUserIdYGameIdCorrectamente()
    {
        var fav = GameFavoriteEntity.Create(userId: 5, gameId: 100);

        fav.UserId.Should().Be(5);
        fav.GameId.Should().Be(100);
    }

    [Fact]
    public void GameFavorite_Create_EstableceCreatedAtCercanaAHora()
    {
        var before = DateTime.UtcNow;
        var fav = GameFavoriteEntity.Create(userId: 1, gameId: 1);
        var after = DateTime.UtcNow;

        fav.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    // ── GamePurchaseEntity ────────────────────────────────────────────────────

    [Fact]
    public void GamePurchase_Create_AsignaUserIdYGameIdCorrectamente()
    {
        var purchase = GamePurchaseEntity.Create(userId: 7, gameId: 200);

        purchase.UserId.Should().Be(7);
        purchase.GameId.Should().Be(200);
    }

    [Fact]
    public void GamePurchase_Create_EstablecePurchasedAtCercanaAHora()
    {
        var before = DateTime.UtcNow;
        var purchase = GamePurchaseEntity.Create(userId: 1, gameId: 1);
        var after = DateTime.UtcNow;

        purchase.PurchasedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}
