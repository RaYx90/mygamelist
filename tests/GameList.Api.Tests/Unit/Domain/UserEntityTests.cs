using FluentAssertions;
using GameList.Domain.Entities;

namespace GameList.Api.Tests.Unit.Domain;

/// <summary>
/// Tests unitarios para <see cref="UserEntity"/>.
/// Verifican las reglas de negocio del dominio: validaciones guard, normalización de email
/// y la lógica de membresía en grupos.
/// No requieren infraestructura ni mocks — son tests puramente en memoria.
/// </summary>
public sealed class UserEntityTests
{
    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ConParametrosValidos_CreaUsuarioCorrectamente()
    {
        var user = UserEntity.Create("alice", "Alice@Test.com", "hash123");

        user.Username.Should().Be("alice");
        user.Email.Should().Be("alice@test.com"); // normalizado a minúsculas
        user.PasswordHash.Should().Be("hash123");
        user.GroupId.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConUsernameVacioOEspacios_LanzaArgumentException(string username)
    {
        var act = () => UserEntity.Create(username, "alice@test.com", "hash");

        act.Should().Throw<ArgumentException>().WithParameterName("username");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConEmailVacioOEspacios_LanzaArgumentException(string email)
    {
        var act = () => UserEntity.Create("alice", email, "hash");

        act.Should().Throw<ArgumentException>().WithParameterName("email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConPasswordHashVacioOEspacios_LanzaArgumentException(string hash)
    {
        var act = () => UserEntity.Create("alice", "alice@test.com", hash);

        act.Should().Throw<ArgumentException>().WithParameterName("passwordHash");
    }

    [Fact]
    public void Create_NormalizaEmailAMinusculas()
    {
        var user = UserEntity.Create("alice", "ALICE@TEST.COM", "hash");

        user.Email.Should().Be("alice@test.com");
    }

    // ── JoinGroup / LeaveGroup ────────────────────────────────────────────────

    [Fact]
    public void JoinGroup_AsignaGroupIdAlUsuario()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");

        user.JoinGroup(42);

        user.GroupId.Should().Be(42);
    }

    [Fact]
    public void LeaveGroup_EstableceGroupIdANull()
    {
        var user = UserEntity.Create("alice", "alice@test.com", "hash");
        user.JoinGroup(42);

        user.LeaveGroup();

        user.GroupId.Should().BeNull();
    }
}
