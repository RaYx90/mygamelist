using FluentAssertions;
using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Auth.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Ports;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="LoginHandler"/> con repositorios mockeados (NSubstitute).
/// Verifican los tres caminos del login: email inexistente, contraseña incorrecta y éxito.
/// </summary>
public sealed class LoginHandlerTests
{
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _sut = new LoginHandler(_userRepo, _hasher);
    }

    [Fact]
    public async Task Handle_ConEmailNoExistente_DevuelveNull()
    {
        _userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        var result = await _sut.Handle(
            new LoginCommand("noexiste@test.com", "pass"), CancellationToken.None);

        result.Should().BeNull();
        // No debe intentar verificar la contraseña si el usuario no existe.
        _hasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ConPasswordIncorrecta_DevuelveNull()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "correct_hash");
        _userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _sut.Handle(
            new LoginCommand("alice@test.com", "wrong_pass"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConCredencialesValidas_DevuelveUserDto()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "correct_hash");
        _userRepo.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        _hasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var result = await _sut.Handle(
            new LoginCommand("alice@test.com", "correct_pass"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
        result.Email.Should().Be("alice@test.com");
        result.GroupId.Should().BeNull();
    }
}
