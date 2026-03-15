using FluentAssertions;
using GameList.Application.Features.Social.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="MarkPurchasedHandler"/> y <see cref="UnmarkPurchasedHandler"/>.
/// Verifican la lógica de idempotencia y la interacción correcta con el repositorio.
/// </summary>
public sealed class MarkUnmarkPurchasedHandlerTests
{
    private readonly IGamePurchaseRepository repo = Substitute.For<IGamePurchaseRepository>();
    private readonly MarkPurchasedHandler markSut;
    private readonly UnmarkPurchasedHandler unmarkSut;

    public MarkUnmarkPurchasedHandlerTests()
    {
        markSut = new MarkPurchasedHandler(repo);
        unmarkSut = new UnmarkPurchasedHandler(repo);
    }

    // ── MarkPurchased ─────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkPurchased_CompraNoExiste_InsertaYGuarda()
    {
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns((GamePurchaseEntity?)null);

        var result = await markSut.Handle(new MarkPurchasedCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        await repo.Received(1).AddAsync(Arg.Any<GamePurchaseEntity>(), Arg.Any<CancellationToken>());
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task MarkPurchased_CompraYaExiste_NoInsertaYDevuelveTrue()
    {
        var existing = GamePurchaseEntity.Create(1, 10);
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await markSut.Handle(new MarkPurchasedCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        await repo.DidNotReceive().AddAsync(Arg.Any<GamePurchaseEntity>(), Arg.Any<CancellationToken>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    // ── UnmarkPurchased ───────────────────────────────────────────────────────

    [Fact]
    public async Task UnmarkPurchased_CompraExiste_EliminaYGuarda()
    {
        var existing = GamePurchaseEntity.Create(1, 10);
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await unmarkSut.Handle(new UnmarkPurchasedCommand(1, 10), CancellationToken.None);

        result.Should().BeTrue();
        repo.Received(1).Remove(existing);
        await repo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UnmarkPurchased_CompraNoExiste_DevuelveFalseSinLanzar()
    {
        repo.GetAsync(1, 10, Arg.Any<CancellationToken>()).Returns((GamePurchaseEntity?)null);

        var result = await unmarkSut.Handle(new UnmarkPurchasedCommand(1, 10), CancellationToken.None);

        result.Should().BeFalse();
        repo.DidNotReceive().Remove(Arg.Any<GamePurchaseEntity>());
        await repo.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
