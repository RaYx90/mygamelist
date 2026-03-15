using FluentAssertions;
using GameList.Application.Features.Social.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="AddFavoriteHandler"/> y <see cref="RemoveFavoriteHandler"/>.
/// Verifican la lógica de idempotencia y la interacción correcta con el repositorio.
/// </summary>
public sealed class AddRemoveFavoriteHandlerTests
{
    private readonly IGameFavoriteRepository repo = Substitute.For<IGameFavoriteRepository>();
    private readonly AddFavoriteHandler addSut;
    private readonly RemoveFavoriteHandler removeSut;

    public AddRemoveFavoriteHandlerTests()
    {
        addSut = new AddFavoriteHandler(repo);
        removeSut = new RemoveFavoriteHandler(repo);
    }

    // ── AddFavorite ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AddFavorite_FavoritoNoExiste_InsertaYGuarda()
    {
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns((GameFavoriteEntity?)null);

        var result = await addSut.Handle(new AddFavoriteCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        await repo.Received(1).AddAsync(Arg.Any<GameFavoriteEntity>(), Arg.Any<CancellationToken>());
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddFavorite_FavoritoYaExiste_NoInsertaYDevuelveTrue()
    {
        var existing = GameFavoriteEntity.Create(1, 10);
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await addSut.Handle(new AddFavoriteCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        await repo.DidNotReceive().AddAsync(Arg.Any<GameFavoriteEntity>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── RemoveFavorite ────────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveFavorite_FavoritoExiste_EliminaYGuarda()
    {
        var existing = GameFavoriteEntity.Create(1, 10);
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await removeSut.Handle(new RemoveFavoriteCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        repo.Received(1).Remove(existing);
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemoveFavorite_FavoritoNoExiste_DevuelveFalseSinLanzar()
    {
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns((GameFavoriteEntity?)null);

        var result = await removeSut.Handle(new RemoveFavoriteCommand(1, 10), CancellationToken.None);

        result.Should().BeFalse();
        repo.DidNotReceive().Remove(Arg.Any<GameFavoriteEntity>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
