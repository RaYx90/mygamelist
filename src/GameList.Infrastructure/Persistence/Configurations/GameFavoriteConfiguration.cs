using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

internal sealed class GameFavoriteConfiguration : IEntityTypeConfiguration<GameFavoriteEntity>
{
    public void Configure(EntityTypeBuilder<GameFavoriteEntity> builder)
    {
        builder.ToTable("GameFavorites");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).ValueGeneratedOnAdd();
        builder.HasIndex(f => new { f.UserId, f.GameId }).IsUnique();
        builder.Property(f => f.CreatedAt).IsRequired();
        builder.HasOne(f => f.User).WithMany(u => u.Favorites).HasForeignKey(f => f.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(f => f.Game).WithMany().HasForeignKey(f => f.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
