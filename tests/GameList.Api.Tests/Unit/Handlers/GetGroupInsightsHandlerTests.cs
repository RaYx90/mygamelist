using FluentAssertions;
using GameList.Application.Features.Social.Queries;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="GetGroupInsightsHandler"/>.
/// Solo hay coincidencia cuando: más de 1 persona desea el juego, más de 1 lo ha comprado,
/// o al menos 1 lo desea Y al menos 1 lo ha comprado.
/// </summary>
public sealed class GetGroupInsightsHandlerTests
{
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly IGameFavoriteRepository favRepo = Substitute.For<IGameFavoriteRepository>();
    private readonly IGamePurchaseRepository purchaseRepo = Substitute.For<IGamePurchaseRepository>();
    private readonly GetGroupInsightsHandler sut;

    public GetGroupInsightsHandlerTests()
    {
        sut = new GetGroupInsightsHandler(userRepo, favRepo, purchaseRepo);
    }

    [Fact]
    public async Task Handle_UsuarioSinGrupo_DevuelveListaVacia()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));

        var result = await sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
        await favRepo.DidNotReceive().GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SoloUnUsuarioConFavorito_NoEsCoincidencia()
    {
        // Un único miembro con el juego en favoritos → no es coincidencia.
        var alice = UserEntity.Create("alice", "alice@test.com", "hash");
        alice.JoinGroup(42);
        var fav = GameFavoriteEntity.Create(userId: 0, gameId: 100);

        SetupGroup(42, alice);
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GameFavoriteEntity>>(new[] { fav }));
        purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GamePurchaseEntity>>(Array.Empty<GamePurchaseEntity>()));

        var result = await sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_DosUsuariosConMismoFavorito_EsCoincidencia()
    {
        // Dos miembros del grupo marcan el mismo juego como favorito → coincidencia.
        var alice = UserEntity.Create("alice", "alice@test.com", "hash");
        var bob = UserEntity.Create("bob", "bob@test.com", "hash");
        alice.JoinGroup(42);
        bob.JoinGroup(42);

        // alice.Id = 0, bob.Id = 0 en entidades sin persistir → ambos caen al mismo userId=0.
        // Usamos GameFavoriteEntity con userId distintos para simular dos personas.
        var favAlice = GameFavoriteEntity.Create(userId: 0, gameId: 100);
        var favBob = GameFavoriteEntity.Create(userId: 1, gameId: 100);

        SetupGroupWithTwo(42, alice, bob);
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GameFavoriteEntity>>(new[] { favAlice, favBob }));
        purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GamePurchaseEntity>>(Array.Empty<GamePurchaseEntity>()));

        var result = await sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].GameId.Should().Be(100);
        result[0].WantedBy.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_MismaPersonaFavoritoYCompra_NoEsCoincidencia()
    {
        // La misma persona marca favorito Y compra → no es coincidencia (solo 1 persona distinta).
        var alice = UserEntity.Create("alice", "alice@test.com", "hash");
        alice.JoinGroup(42);

        var fav = GameFavoriteEntity.Create(userId: 0, gameId: 100);
        var purchase = GamePurchaseEntity.Create(userId: 0, gameId: 100);

        SetupGroup(42, alice);
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GameFavoriteEntity>>(new[] { fav }));
        purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GamePurchaseEntity>>(new[] { purchase }));

        var result = await sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_UnFavoritoDeUnUsuarioYCompraDeOtro_EsCoincidencia()
    {
        // Una persona lo desea y una persona DISTINTA lo ha comprado → coincidencia.
        var alice = UserEntity.Create("alice", "alice@test.com", "hash");
        var bob = UserEntity.Create("bob", "bob@test.com", "hash");
        alice.JoinGroup(42);
        bob.JoinGroup(42);

        var fav = GameFavoriteEntity.Create(userId: 0, gameId: 100);      // alice
        var purchase = GamePurchaseEntity.Create(userId: 1, gameId: 100); // bob

        SetupGroupWithTwo(42, alice, bob);
        favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GameFavoriteEntity>>(new[] { fav }));
        purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GamePurchaseEntity>>(new[] { purchase }));

        var result = await sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].WantedBy.Should().ContainSingle();
        result[0].PurchasedBy.Should().ContainSingle();
    }

    private void SetupGroup(int groupId, UserEntity member)
    {
        userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(member));
        userRepo.GetByGroupIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserEntity>>(new[] { member }));
    }

    private void SetupGroupWithTwo(int groupId, UserEntity m1, UserEntity m2)
    {
        userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(m1));
        userRepo.GetByGroupIdAsync(groupId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserEntity>>(new[] { m1, m2 }));
    }
}
