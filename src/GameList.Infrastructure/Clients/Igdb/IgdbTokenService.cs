using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GameList.Infrastructure.Clients.Igdb;

/// <summary>
/// Handles OAuth2 Client Credentials token acquisition and caching for Twitch/IGDB.
/// </summary>
internal sealed class IgdbTokenService
{
    private readonly HttpClient _httpClient;
    private readonly IgdbOptionsConfig _options;

    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public IgdbTokenService(HttpClient httpClient, IgdbOptionsConfig options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        await _lock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
                return _cachedToken;

            var response = await _httpClient.PostAsync(
                $"{_options.TokenUrl}?client_id={_options.ClientId}" +
                $"&client_secret={_options.ClientSecret}" +
                $"&grant_type=client_credentials",
                content: null,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content
                .ReadFromJsonAsync<IgdbTokenResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize IGDB token response.");

            _cachedToken = tokenResponse.AccessToken;
            // Refresh 60 seconds before actual expiry
            _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return _cachedToken;
        }
        finally
        {
            _lock.Release();
        }
    }
}

internal sealed record IgdbTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType
);
