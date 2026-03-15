using FluentAssertions;
using GameList.Application.Features.Auth.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Exceptions;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="ChangeUsernameHandler"/>.
/// Verifica que detecta usuario inexistente, nombre duplicado y actualización correcta.
/// </summary>
public sealed class ChangeUsernameHandlerTests
{
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly ChangeUsernameHandler sut;

    public ChangeUsernameHandlerTests()
    {
        sut = new ChangeUsernameHandler(userRepo);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_LanzaConflictException()
    {
        userRepo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((UserEntity?)null);

        var act = () => sut.Handle(new ChangeUsernameCommand(99, "nuevonombre"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_NombreDuplicadoPorOtroUsuario_LanzaConflictException()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hashed");
        userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        // El nombre "bob" ya existe y es de otro usuario
        userRepo.ExistsByUsernameAsync("bob", Arg.Any<CancellationToken>()).Returns(true);

        var act = () => sut.Handle(new ChangeUsernameCommand(1, "bob"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>().WithMessage("*bob*");
    }

    [Fact]
    public async Task Handle_MismoNombreDelUsuario_ActualizaSinConflicto()
    {
        // Cambiar al mismo nombre que ya tiene es válido (ExistsByUsername devuelve true pero es el mismo usuario)
        var user = UserEntity.Create("alice", "alice@test.com", "hashed");
        userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        userRepo.ExistsByUsernameAsync("alice", Arg.Any<CancellationToken>()).Returns(true);

        // No debe lanzar
        var result = await sut.Handle(new ChangeUsernameCommand(1, "alice"), CancellationToken.None);

        result.Should().Be("alice");
        userRepo.Received(1).Update(user);
        await userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NombreValido_ActualizaYDevuelveNuevoNombre()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hashed");
        userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);
        userRepo.ExistsByUsernameAsync("alicia", Arg.Any<CancellationToken>()).Returns(false);

        var result = await sut.Handle(new ChangeUsernameCommand(1, "alicia"), CancellationToken.None);

        result.Should().Be("alicia");
        userRepo.Received(1).Update(user);
        await userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
