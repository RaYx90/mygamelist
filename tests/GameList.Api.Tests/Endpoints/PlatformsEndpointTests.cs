using GameList.Application.Features.Sync.Commands;
using GameList.Api.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GameList.Api.Tests.Endpoints;

public sealed class PlatformsEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public PlatformsEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Los endpoints de plataformas requieren autenticación JWT.
    public async Task InitializeAsync()
    {
        var (token, _) = await TestHelpers.RegisterAndLoginAsync(_client);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task SeedDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        await sender.Send(new SyncGamesCommand(DateTime.UtcNow.Year));
    }

    [Fact]
    public async Task GetPlatforms_AfterSync_Returns200WithPlatforms()
    {
        await SeedDataAsync();

        var response = await _client.GetAsync("/api/platforms");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        doc.RootElement.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPlatforms_Returns200_EvenWhenEmpty()
    {
        var response = await _client.GetAsync("/api/platforms");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
