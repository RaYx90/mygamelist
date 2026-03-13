using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de EF Core para la entidad <see cref="GroupEntity"/>.
/// </summary>
internal sealed class GroupConfiguration : IEntityTypeConfiguration<GroupEntity>
{
    /// <summary>
    /// Define el mapeo de columnas e índices de la tabla Groups.
    /// </summary>
    /// <param name="builder">Constructor de configuración de la entidad.</param>
    public void Configure(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedOnAdd();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.InviteCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(g => g.InviteCode).IsUnique();
        builder.Property(g => g.CreatedAt).IsRequired();
    }
}
