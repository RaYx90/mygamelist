using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

internal sealed class GameConfiguration : IEntityTypeConfiguration<GameEntity>
{
    public void Configure(EntityTypeBuilder<GameEntity> builder)
    {
        builder.ToTable("Games");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id)
            .ValueGeneratedOnAdd();

        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(g => g.Slug)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(g => g.Slug)
            .IsUnique();

        builder.Property(g => g.IgdbId)
            .IsRequired();

        builder.HasIndex(g => g.IgdbId)
            .IsUnique();

        builder.Property(g => g.Summary)
            .HasMaxLength(5000);

        builder.Property(g => g.CoverImageUrl)
            .HasMaxLength(1000);

        builder.Property(g => g.Category)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(g => g.IsIndie)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(g => g.CreatedAt)
            .IsRequired();

        builder.Property(g => g.UpdatedAt)
            .IsRequired();

        builder.HasMany(g => g.Releases)
            .WithOne(r => r.Game)
            .HasForeignKey(r => r.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
