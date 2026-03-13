using FluentAssertions;
using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="LoginHandler"/> con repositorios mockeados (NSubstitute).
/// Verifican los tres caminos del login: email inexistente, contraseña incorrecta y éxito.
/// </summary>
public sealed class LoginHandlerTests
{
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher hasher = Substitute.For<IPasswordHasher>();
    private readonly LoginHandler sut;

    public LoginHandlerTests()
    {
        sut = new LoginHandler(userRepo, hasher);
    }

    [Fact]
    public async Task Handle_ConEmailNoExistente_DevuelveNull()
    {
        userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        var result = await sut.Handle(
            new LoginCommand("noexiste@test.com", "pass"), CancellationToken.None);

        result.Should().BeNull();
        // No debe intentar verificar la contraseña si el usuario no existe.
        hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ConPasswordIncorrecta_DevuelveNull()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "correct_hash");
        userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await sut.Handle(
            new LoginCommand("alice@test.com", "wrong_pass"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConCredencialesValidas_DevuelveUserDto()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "correct_hash");
        userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var result = await sut.Handle(
            new LoginCommand("alice@test.com", "correct_pass"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
        result.Email.Should().Be("alice@test.com");
        result.GroupId.Should().BeNull();
    }
}
