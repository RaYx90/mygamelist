using FluentAssertions;
using GameList.Application.Features.Social.Commands;
using GameList.Domain.Entities;
using GameList.Domain.Ports;
using NSubstitute;
using System.Text.RegularExpressions;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="CreateGroupHandler"/> con repositorios mockeados.
/// Verifica que el grupo se crea con un código de invitación válido y que el creador
/// queda registrado como primer miembro.
/// </summary>
public sealed class CreateGroupHandlerTests
{
    private readonly IGroupRepository _groupRepo = Substitute.For<IGroupRepository>();
    private readonly IUserRepository _userRepo = Substitute.For<IUserRepository>();
    private readonly CreateGroupHandler _sut;

    public CreateGroupHandlerTests()
    {
        _sut = new CreateGroupHandler(_groupRepo, _userRepo);
    }

    [Fact]
    public async Task Handle_UsuarioNoExistente_LanzaInvalidOperationException()
    {
        _userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        var act = () => _sut.Handle(
            new CreateGroupCommand(999, "Mi Grupo"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ConUsuarioValido_DevuelveGroupDtoConCodigoAlphanumericoYCreadorComoMiembro()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        _userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));

        var result = await _sut.Handle(
            new CreateGroupCommand(1, "Los Gamers"), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Los Gamers");

        // El código de invitación debe tener exactamente 8 caracteres alfanuméricos en mayúsculas.
        result.InviteCode.Should().HaveLength(8);
        result.InviteCode.Should().MatchRegex("^[A-Z0-9]{8}$");

        // El creador debe ser el único miembro inicial.
        result.MemberUsernames.Should().ContainSingle().Which.Should().Be("alice");

        // Verificar que se persistió el grupo y la actualización del usuario.
        await _groupRepo.Received(1).AddAsync(Arg.Any<GroupEntity>(), Arg.Any<CancellationToken>());
        await _groupRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        _userRepo.Received(1).Update(user);
        await _userRepo.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
