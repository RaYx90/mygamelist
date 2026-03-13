using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence;

/// <summary>
/// Contexto de base de datos de la aplicación. Registra todas las entidades y aplica las configuraciones Fluent API.
/// </summary>
public sealed class AppDbContext : DbContext
{
    /// <summary>Conjunto de juegos.</summary>
    public DbSet<GameEntity> Games => Set<GameEntity>();

    /// <summary>Conjunto de plataformas.</summary>
    public DbSet<PlatformEntity> Platforms => Set<PlatformEntity>();

    /// <summary>Conjunto de lanzamientos de juegos por plataforma.</summary>
    public DbSet<GameReleaseEntity> GameReleases => Set<GameReleaseEntity>();

    /// <summary>Conjunto de usuarios registrados.</summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>Conjunto de grupos sociales.</summary>
    public DbSet<GroupEntity> Groups => Set<GroupEntity>();

    /// <summary>Conjunto de juegos favoritos de usuarios.</summary>
    public DbSet<GameFavoriteEntity> GameFavorites => Set<GameFavoriteEntity>();

    /// <summary>Conjunto de juegos marcados como comprados por usuarios.</summary>
    public DbSet<GamePurchaseEntity> GamePurchases => Set<GamePurchaseEntity>();

    /// <summary>
    /// Inicializa el contexto con las opciones de configuración de EF Core.
    /// </summary>
    /// <param name="options">Opciones del contexto de base de datos.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
