using FluentAssertions;
using GameList.Application.Features.Auth.Queries;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="GetCurrentUserHandler"/>.
/// Verifica que devuelve null cuando el usuario no existe y el DTO correcto cuando existe.
/// </summary>
public sealed class GetCurrentUserHandlerTests
{
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly GetCurrentUserHandler sut;

    public GetCurrentUserHandlerTests()
    {
        sut = new GetCurrentUserHandler(userRepo);
    }

    [Fact]
    public async Task Handle_UsuarioNoExiste_DevuelveNull()
    {
        userRepo.GetByIdAsync(99, Arg.Any<CancellationToken>()).Returns((UserEntity?)null);

        var result = await sut.Handle(new GetCurrentUserQuery(99), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UsuarioExiste_DevuelveUserDtoConDatosCorrectos()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hashed");
        userRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(user);

        var result = await sut.Handle(new GetCurrentUserQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Username.Should().Be("alice");
        result.Email.Should().Be("alice@test.com");
        result.GroupId.Should().BeNull();
    }
}
