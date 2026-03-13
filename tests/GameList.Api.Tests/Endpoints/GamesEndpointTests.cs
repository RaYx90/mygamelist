using GameList.Application.Features.Sync.Commands;
using GameList.Api.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Net.Http.Headers;

namespace GameList.Api.Tests.Endpoints;

public sealed class GamesEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient client;
    private readonly CustomWebApplicationFactory factory;

    public GamesEndpointTests(CustomWebApplicationFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
    }

    // Los endpoints de juegos requieren autenticación JWT.
    // Se registra un usuario único antes de cada test y se configura el header Authorization.
    public async Task InitializeAsync()
    {
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedDataAsync()
    {
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));
    }

    [Fact]
    public async Task GetGameById_ExistingId_Returns200WithGameDetail()
    {
        await SeedDataAsync();

        // Game with database Id=1 (first inserted)
        var response = await client.GetAsync("/api/games/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Game Alpha");
    }

    [Fact]
    public async Task GetGameById_NonExistentId_Returns404()
    {
        var response = await client.GetAsync("/api/games/99999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
