using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public DbSet<GameEntity> Games => Set<GameEntity>();
    public DbSet<PlatformEntity> Platforms => Set<PlatformEntity>();
    public DbSet<GameReleaseEntity> GameReleases => Set<GameReleaseEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<GroupEntity> Groups => Set<GroupEntity>();
    public DbSet<GameFavoriteEntity> GameFavorites => Set<GameFavoriteEntity>();
    public DbSet<GamePurchaseEntity> GamePurchases => Set<GamePurchaseEntity>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
