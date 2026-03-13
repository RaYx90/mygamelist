using FluentAssertions;
using GameList.Application.Features.Social.Queries;
using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using NSubstitute;

namespace GameList.Api.Tests.Unit.Handlers;

/// <summary>
/// Tests unitarios para <see cref="GetMyGroupHandler"/> con repositorios mockeados.
/// Verifica los tres escenarios: usuario inexistente, usuario sin grupo y usuario con grupo.
/// </summary>
public sealed class GetMyGroupHandlerTests
{
    private readonly IUserRepository userRepo = Substitute.For<IUserRepository>();
    private readonly IGroupRepository groupRepo = Substitute.For<IGroupRepository>();
    private readonly GetMyGroupHandler sut;

    public GetMyGroupHandlerTests()
    {
        sut = new GetMyGroupHandler(userRepo, groupRepo);
    }

    [Fact]
    public async Task Handle_UsuarioNoExistente_DevuelveNull()
    {
        userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        var result = await sut.Handle(new GetMyGroupQuery(999), CancellationToken.None);

        result.Should().BeNull();
        // No debe consultar el grupo si el usuario no existe.
        await groupRepo.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UsuarioSinGrupo_DevuelveNull()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        // user.GroupId es null — no pertenece a ningún grupo.
        userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));

        var result = await sut.Handle(new GetMyGroupQuery(1), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_UsuarioConGrupo_DevuelveGroupDtoConMiembros()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        user.JoinGroup(10);

        var group = GroupEntity.Create("Los Gamers", "ABCD1234");
        var member = UserEntity.Create("bob", "bob@test.com", "hash");

        userRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(user));
        groupRepo.GetByIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<GroupEntity?>(group));
        userRepo.GetByGroupIdAsync(10, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserEntity>>(new[] { user, member }));

        var result = await sut.Handle(new GetMyGroupQuery(1), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Los Gamers");
        result.InviteCode.Should().Be("ABCD1234");
        result.MemberUsernames.Should().BeEquivalentTo(new[] { "alice", "bob" });
    }
}
