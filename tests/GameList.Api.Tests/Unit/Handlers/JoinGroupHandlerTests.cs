using FluentAssertions;
using GameList.Application.Features.Social.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Ports;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="JoinGroupHandler"/> con repositorios mockeados (NSubstitute).
/// Verifican los tres escenarios posibles: código inválido, usuario inexistente y unión exitosa.
/// La decisión de devolver null en lugar de lanzar excepción se valida aquí explícitamente.
/// </summary>
public sealed class JoinGroupHandlerTests
{
    private readonly IGroupRepository _groupRepo = Substitute.For<IGroupRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly JoinGroupHandler _sut;

    public JoinGroupHandlerTests()
    {
        _sut = new JoinGroupHandler(_groupRepo, _userRepo);
    }

    [Fact]
    public async Task Handle_ConCodigoDeInvitacionInvalido_DevuelveNull()
    {
        // El repositorio no encuentra ningún grupo con ese código.
        _groupRepo.GetByInviteCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(null));

        var result = await _sut.Handle(
            new JoinGroupCommand(1, "INVALIDO"), CancellationToken.None);

        // null indica código no encontrado — el endpoint lo mapea a 400 Bad Request.
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConUsuarioNoExistente_DevuelveNull()
    {
        // El código es válido pero el userId no existe (no debería ocurrir en producción).
        var group = GroupEntity.Create("Test Group", "ABCD1234");
        _groupRepo.GetByInviteCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(group));
        _userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        var result = await _sut.Handle(
            new JoinGroupCommand(999, "ABCD1234"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ConDatosValidos_UnealUsuarioAlGrupoYDevuelveGroupDto()
    {
        var group = GroupEntity.Create("Test Group", "ABCD1234");
        var user = UserEntity.Create("alice", "alice@test.com", "hash");

        _groupRepo.GetByInviteCodeAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(group));
        _userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        // Simula los miembros del grupo tras el join (solo "alice" en este caso).
        _userRepo.GetByGroupIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserEntity>>(new[] { user }));

        var result = await _sut.Handle(
            new JoinGroupCommand(1, "ABCD1234"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Group");
        result.InviteCode.Should().Be("ABCD1234");
        result.MemberUsernames.Should().ContainSingle().Which.Should().Be("alice");

        // Verificar que se persistió el cambio.
        _userRepo.Received(1).Update(user);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
