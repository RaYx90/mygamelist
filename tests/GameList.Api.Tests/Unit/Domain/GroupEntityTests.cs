using FluentAssertions;
using GameList.Domain.Entities;

namespace GameList.Api.Tests.Unit.Domain;

/// <summary>
/// Tests unitarios para <see cref="GroupEntity"/>.
/// Verifican las validaciones guard del método de fábrica y la inmutabilidad del estado.
/// </summary>
public sealed class GroupEntityTests
{
    [Fact]
    public void Create_ConParametrosValidos_CreaGrupoCorrectamente()
    {
        var group = GroupEntity.Create("Los Gamers", "ABCD1234");

        group.Name.Should().Be("Los Gamers");
        group.InviteCode.Should().Be("ABCD1234");
        group.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        group.Members.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ConNombreVacioOEspacios_LanzaArgumentException(string name)
    {
        var act = () => GroupEntity.Create(name, "ABCD1234");

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void Create_ConNombreConEspaciosExtremos_LosElimina()
    {
        var group = GroupEntity.Create("  Los Gamers  ", "ABCD1234");

        group.Name.Should().Be("Los Gamers");
    }
}
