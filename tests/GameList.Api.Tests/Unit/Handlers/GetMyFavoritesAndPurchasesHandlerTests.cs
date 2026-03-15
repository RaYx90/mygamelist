using FluentAssertions;
using GameList.Application.Features.Social.Queries;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="GetMyFavoritesHandler"/> y <see cref="GetMyPurchasesHandler"/>.
/// Verifican la interacción con el repositorio y el filtrado de entidades sin navegación cargada.
/// </summary>
public sealed class GetMyFavoritesAndPurchasesHandlerTests
{
    private readonly IGameFavoriteRepository favRepo = Substitute.For<IGameFavoriteRepository>();
    private readonly IGamePurchaseRepository purchaseRepo = Substitute.For<IGamePurchaseRepository>();
    private readonly GetMyFavoritesHandler favSut;
    private readonly GetMyPurchasesHandler purchaseSut;

    public GetMyFavoritesAndPurchasesHandlerTests()
    {
        favSut = new GetMyFavoritesHandler(favRepo);
        purchaseSut = new GetMyPurchasesHandler(purchaseRepo);
    }

    // ── GetMyFavorites ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyFavorites_SinFavoritos_DevuelveListaVacia()
    {
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameFavoriteEntity>().AsReadOnly());

        var result = await favSut.Handle(new GetMyFavoritesQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
        await favRepo.Received(1).GetByUserIdsAsync(
            Arg.Is<IReadOnlyList<int>>(ids => ids.Contains(1)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMyFavorites_ConFavoritosSinNavegacionGame_FiltradosYDevuelveVacio()
    {
        // Los favoritos sin la navegación a Game (Game == null) se filtran
        var favWithoutGame = GameFavoriteEntity.Create(1, 42);
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(new List<GameFavoriteEntity> { favWithoutGame }.AsReadOnly());

        var result = await favSut.Handle(new GetMyFavoritesQuery(1), CancellationToken.None);

        // Game == null → filtrado → lista vacía
        result.Should().BeEmpty();
    }

    // ── GetMyPurchases ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMyPurchases_SinCompras_DevuelveListaVacia()
    {
        purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(new List<GamePurchaseEntity>().AsReadOnly());

        var result = await purchaseSut.Handle(new GetMyPurchasesQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
        await purchaseRepo.Received(1).GetByUserIdsAsync(
            Arg.Is<IReadOnlyList<int>>(ids => ids.Contains(1)),
            Arg.Any<CancellationToken>());
    }
}
