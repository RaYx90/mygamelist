using GameList.Domain.Ports;
using GameList.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace GameList.Api.Tests.Common;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _pgContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .Build();

    public async Task InitializeAsync()
    {
        await _pgContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace IGDB adapter with fake
            services.RemoveAll<IGameDataProvider>();
            services.AddScoped<IGameDataProvider, FakeGameDataProvider>();

            // Replace PostgreSQL connection with container's
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_pgContainer.GetConnectionString()));

            // Remove background sync to avoid interference
            services.RemoveAll<Microsoft.Extensions.Hosting.IHostedService>();
        });
    }
}
