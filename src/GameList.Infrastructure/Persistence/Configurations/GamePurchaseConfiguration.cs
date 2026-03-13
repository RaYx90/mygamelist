using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuración Fluent API de EF Core para la entidad <see cref="GamePurchaseEntity"/>.
/// </summary>
internal sealed class GamePurchaseConfiguration : IEntityTypeConfiguration<GamePurchaseEntity>
{
    /// <summary>
    /// Define el mapeo de columnas, índice de unicidad y relaciones de la tabla GamePurchases.
    /// </summary>
    /// <param name="builder">Constructor de configuración de la entidad.</param>
    public void Configure(EntityTypeBuilder<GamePurchaseEntity> builder)
    {
        builder.ToTable("GamePurchases");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();
        builder.HasIndex(p => new { p.UserId, p.GameId }).IsUnique();
        builder.Property(p => p.PurchasedAt).IsRequired();
        builder.HasOne(p => p.User).WithMany(u => u.Purchases).HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(p => p.Game).WithMany().HasForeignKey(p => p.GameId).OnDelete(DeleteBehavior.Cascade);
    }
}
