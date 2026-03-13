using FluentAssertions;
using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.Commands;
using GameList.Domain.Exceptions;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="RegisterHandler"/> con repositorios mockeados (NSubstitute).
/// Verifican la lógica de negocio: detección de duplicados y registro exitoso,
/// sin necesidad de base de datos real.
/// </summary>
public sealed class RegisterHandlerTests
{
    // Mocks de dependencias — creados una vez por clase de test.
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher hasher = Substitute.For<IPasswordHasher>();
    private readonly RegisterHandler sut;

    public RegisterHandlerTests()
    {
        sut = new RegisterHandler(userRepo, hasher);
    }

    [Fact]
    public async Task Handle_ConEmailDuplicado_LanzaConflictException()
    {
        // El repositorio simula que el email ya existe en la BD.
        userRepo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var act = () => sut.Handle(
            new RegisterCommand("alice", "alice@test.com", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*alice@test.com*");
    }

    [Fact]
    public async Task Handle_ConUsernameDuplicado_LanzaConflictException()
    {
        // Email libre, pero username ya en uso.
        userRepo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        userRepo.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        var act = () => sut.Handle(
            new RegisterCommand("alice", "alice@test.com", "pass"), CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*alice*");
    }

    [Fact]
    public async Task Handle_ConDatosValidos_DevuelveUserDtoConDatosNormalizados()
    {
        // Sin duplicados — el registro debe completarse.
        userRepo.ExistsByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        userRepo.ExistsByUsernameAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));
        hasher.Hash(Arg.Any<string>()).Returns("hashed_password");

        var result = await sut.Handle(
            new RegisterCommand("alice", "Alice@Test.com", "pass"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Username.Should().Be("alice");
        // El email se normaliza a minúsculas dentro de UserEntity.Create
        result.Email.Should().Be("alice@test.com");
        result.GroupId.Should().BeNull();

        // Verificar que se llamó a AddAsync y SaveChangesAsync exactamente una vez.
        await userRepo.Received(1).AddAsync(Arg.Any<GameList.Domain.Entities.UserEntity>(), Arg.Any<CancellationToken>());
        await userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
