using GameList.Domain.Entities;
using GameList.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de EF Core para la entidad <see cref="GameReleaseEntity"/>.
/// </summary>
internal sealed class GameReleaseConfiguration : IEntityTypeConfiguration<GameReleaseEntity>
{
    /// <summary>
    /// Define el mapeo de columnas, índices y relaciones de la tabla GameReleases.
    /// </summary>
    /// <param name="builder">Constructor de configuración de la entidad.</param>
    public void Configure(EntityTypeBuilder<GameReleaseEntity> builder)
    {
        builder.ToTable("GameReleases");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.GameId)
            .IsRequired();

        builder.Property(r => r.PlatformId)
            .IsRequired();

        builder.Property(r => r.ReleaseDate)
            .IsRequired();

        builder.HasIndex(r => r.ReleaseDate);

        builder.Property(r => r.ReleaseType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Region)
            .HasMaxLength(100);

        builder.HasOne(r => r.Platform)
            .WithMany()
            .HasForeignKey(r => r.PlatformId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.GameId, r.PlatformId })
            .IsUnique();
    }
}
