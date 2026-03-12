using GameList.Application.Features.Sync.Commands;
using GameList.Api.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Net.Http.Headers;

namespace GameList.Api.Tests.Endpoints;

public sealed class ReleasesEndpointTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ReleasesEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // Los endpoints de lanzamientos requieren autenticación JWT.
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
    public async Task GetReleasesByMonth_ValidMonth_Returns200WithData()
    {
        await SeedDataAsync();
        var year = DateTime.UtcNow.Year;

        var response = await _client.GetAsync($"/api/releases?year={year}&month=3");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetReleasesByMonth_WithPlatformFilter_ReturnsFilteredResults()
    {
        await SeedDataAsync();
        var year = DateTime.UtcNow.Year;

        // First get platforms to get a valid ID
        var platformsResponse = await _client.GetAsync("/api/platforms");
        platformsResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var response = await _client.GetAsync($"/api/releases?year={year}&month=3&platformId=1");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetReleasesByMonth_MonthWithNoReleases_Returns200WithEmptyList()
    {
        await SeedDataAsync();
        var year = DateTime.UtcNow.Year;

        // Month 6 has no seeded releases
        var response = await _client.GetAsync($"/api/releases?year={year}&month=6");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("[]");
    }

    [Fact]
    public async Task GetReleasesByMonth_WithoutParameters_UsesCurrentMonthAndYear()
    {
        await SeedDataAsync();

        var response = await _client.GetAsync("/api/releases");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
