using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de EF Core para la entidad <see cref="UserEntity"/>.
/// </summary>
internal sealed class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// Define el mapeo de columnas, índices y relaciones de la tabla Users.
    /// </summary>
    /// <param name="builder">Constructor de configuración de la entidad.</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.Property(u => u.Username).IsRequired().HasMaxLength(100);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.HasOne(u => u.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(u => u.GroupId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
