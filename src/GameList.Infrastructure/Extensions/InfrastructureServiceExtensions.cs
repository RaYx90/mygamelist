using GameList.Application.Common.Interfaces;
using GameList.Domain.Ports;
using GameList.Infrastructure.Auth;
using GameList.Infrastructure.BackgroundServices;
using GameList.Infrastructure.Clients.Igdb;
using GameList.Infrastructure.Persistence;
using GameList.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GameList.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
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

        // Background service
        services.AddHostedService<SyncBackgroundService>();

        return services;
    }
}
