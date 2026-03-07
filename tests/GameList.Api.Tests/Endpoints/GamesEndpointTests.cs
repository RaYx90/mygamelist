using GameList.Application.Features.Sync.Commands;
using GameList.Api.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace GameList.Api.Tests.Endpoints;

public sealed class GamesEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public GamesEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));
    }

    [Fact]
    public async Task GetGameById_ExistingId_Returns200WithGameDetail()
    {
        await SeedDataAsync();

        // Game with database Id=1 (first inserted)
        var response = await _client.GetAsync("/api/games/1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Test Game Alpha");
    }

    [Fact]
    public async Task GetGameById_NonExistentId_Returns404()
    {
        var response = await _client.GetAsync("/api/games/99999");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
