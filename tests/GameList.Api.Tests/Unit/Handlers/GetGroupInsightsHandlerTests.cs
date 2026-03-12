using FluentAssertions;
using GameList.Application.Features.Social.Queries;
using GameList.Domain.Entities;
using GameList.Domain.Ports;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="GetGroupInsightsHandler"/> con repositorios mockeados.
/// Verifican la lógica de agregación: usuario sin grupo devuelve lista vacía,
/// y usuario con favoritos devuelve insights correctamente agregados.
/// Nota: las propiedades de navegación Game de los favoritos serán null en los mocks
/// (EF Core no las carga fuera del contexto), por lo que el nombre del juego
/// cae al fallback "Juego #{gameId}" — esto es comportamiento esperado y válido.
/// </summary>
public sealed class GetGroupInsightsHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IGameFavoriteRepository _favRepo = Substitute.For<IGameFavoriteRepository>();
    private readonly IGamePurchaseRepository _purchaseRepo = Substitute.For<IGamePurchaseRepository>();
    private readonly GetGroupInsightsHandler _sut;

    public GetGroupInsightsHandlerTests()
    {
        _sut = new GetGroupInsightsHandler(_userRepo, _favRepo, _purchaseRepo);
    }

    [Fact]
    public async Task Handle_UsuarioSinGrupo_DevuelveListaVacia()
    {
        // El usuario existe pero no pertenece a ningún grupo (GroupId = null).
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        _userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));

        var result = await _sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().BeEmpty();
        // No debe consultar favoritos si el usuario no tiene grupo.
        await _favRepo.DidNotReceive().GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ConFavoritoDeGrupo_DevuelveInsightConUsernameEnWantedBy()
    {
        // Usuario miembro de un grupo con gameId=100 marcado como favorito.
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        user.JoinGroup(42);

        // user.Id = 0 porque EF Core no lo ha asignado (entidad sin persistir).
        // fav.UserId = 0 coincide con user.Id = 0 → el lookup en usernameById funciona.
        var fav = GameFavoriteEntity.Create(userId: 0, gameId: 100);

        _userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        _userRepo.GetByGroupIdAsync(42, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserEntity>>(new[] { user }));
        _favRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GameFavoriteEntity>>(new[] { fav }));
        _purchaseRepo.GetByUserIdsAsync(Arg.Any<IReadOnlyList<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<GamePurchaseEntity>>(Array.Empty<GamePurchaseEntity>()));

        var result = await _sut.Handle(new GetGroupInsightsQuery(1), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].GameId.Should().Be(100);
        // "alice" aparece en WantedBy porque tiene el juego 100 como favorito.
        result[0].WantedBy.Should().ContainSingle().Which.Should().Be("alice");
        result[0].PurchasedBy.Should().BeEmpty();
    }
}
