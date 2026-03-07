using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

internal sealed class PlatformConfiguration : IEntityTypeConfiguration<PlatformEntity>
{
    public void Configure(EntityTypeBuilder<PlatformEntity> builder)
    {
        builder.ToTable("Platforms");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.IgdbId)
            .IsRequired();

        builder.HasIndex(p => p.IgdbId)
            .IsUnique();

        builder.Property(p => p.Abbreviation)
            .HasMaxLength(50);
    }
}
