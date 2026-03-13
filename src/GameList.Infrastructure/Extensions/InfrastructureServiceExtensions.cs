using GameList.Application.Common.Interfaces;
using GameList.Domain.Interfaces;
using GameList.Infrastructure.Auth;
using GameList.Infrastructure.BackgroundServices;
using GameList.Infrastructure.Clients.Igdb;
using GameList.Infrastructure.Clients.LibreTranslate;
using GameList.Infrastructure.Persistence;
using GameList.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameList.Infrastructure.Extensions;

/// <summary>
/// Extensiones de <see cref="IServiceCollection"/> para registrar todos los servicios de infraestructura.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registra en el contenedor de DI todos los servicios de infraestructura:
    /// base de datos, repositorios, clientes HTTP (IGDB, LibreTranslate), autenticación y servicios en segundo plano.
    /// </summary>
    /// <param name="services">Colección de servicios donde se registran las dependencias.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>La misma instancia de <see cref="IServiceCollection"/> para encadenamiento.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null)));

        // Repositories
        services.AddScoped<IGameRepository, GameRepository>();
        services.AddScoped<IPlatformRepository, PlatformRepository>();
        services.AddScoped<IGameReleaseRepository, GameReleaseRepository>();

        // IGDB Options
        services.Configure<IgdbOptionsConfig>(
            configuration.GetSection(IgdbOptionsConfig.SectionName));

        // IGDB Token Service (singleton — caches the token)
        services.AddHttpClient(nameof(IgdbTokenService));
        services.AddSingleton<IgdbTokenService>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<IgdbOptionsConfig>>().Value;
            return new IgdbTokenService(factory.CreateClient(nameof(IgdbTokenService)), options);
        });

        // IGDB Data Provider with standard resilience (retry + circuit breaker + timeout)
        services.AddHttpClient<IGameDataProvider, IgdbDataProviderAdapter>()
            .AddStandardResilienceHandler();

        // LibreTranslate (contenedor propio, sin clave, sin límites)
        services.Configure<LibreTranslateOptionsConfig>(
            configuration.GetSection(LibreTranslateOptionsConfig.SectionName));
        services.AddHttpClient<ITranslationService, LibreTranslateAdapter>();

        // Application services
        services.AddScoped<IGameSyncService, GameSyncService>();

        // Auth
        services.Configure<JwtOptionsConfig>(configuration.GetSection(JwtOptionsConfig.SectionName));
        services.Configure<RegistrationOptionsConfig>(configuration.GetSection(RegistrationOptionsConfig.SectionName));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Social repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGameFavoriteRepository, GameFavoriteRepository>();
        services.AddScoped<IGamePurchaseRepository, GamePurchaseRepository>();

        // Background services
        services.AddHostedService<SyncBackgroundService>();
        services.AddHostedService<TranslationBackgroundService>();

        return services;
    }
}
