using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GameList.Infrastructure.Clients.Igdb;

/// <summary>
/// Gestiona la obtención y caché del token de acceso OAuth2 para la API de IGDB (Twitch).
/// Usa doble comprobación con semáforo para evitar solicitudes concurrentes de token.
/// </summary>
internal sealed class IgdbTokenService
{
    private readonly HttpClient httpClient;
    private readonly IgdbOptionsConfig options;

    private string? cachedToken;
    private DateTime tokenExpiry = DateTime.MinValue;
    private readonly SemaphoreSlim tokenLock = new(1, 1);

    /// <summary>
    /// Inicializa el servicio con el cliente HTTP y las opciones de configuración de IGDB.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para las solicitudes de token.</param>
    /// <param name="options">Opciones de configuración de IGDB.</param>
    public IgdbTokenService(HttpClient httpClient, IgdbOptionsConfig options)
    {
        this.httpClient = httpClient;
        this.options = options;
    }

    /// <summary>
    /// Devuelve el token de acceso vigente, solicitando uno nuevo a Twitch si ha expirado.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Token de acceso OAuth2 como cadena.</returns>
    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        if (cachedToken is not null && DateTime.UtcNow < tokenExpiry)
            return cachedToken;

        await tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (cachedToken is not null && DateTime.UtcNow < tokenExpiry)
                return cachedToken;

            var response = await httpClient.PostAsync(
                $"{options.TokenUrl}?client_id={options.ClientId}" +
                $"&client_secret={options.ClientSecret}" +
                $"&grant_type=client_credentials",
                content: null,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var tokenResponse = await response.Content
                .ReadFromJsonAsync<IgdbTokenResponse>(cancellationToken: cancellationToken)
                ?? throw new InvalidOperationException("Failed to deserialize IGDB token response.");

            cachedToken = tokenResponse.AccessToken;
            // Refresh 60 seconds before actual expiry
            tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

            return cachedToken;
        }
        finally
        {
            tokenLock.Release();
        }
    }
}

/// <summary>
/// Modelo de respuesta del endpoint de tokens OAuth2 de Twitch.
/// </summary>
internal sealed record IgdbTokenResponse(
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn,
    [property: JsonPropertyName("token_type")] string TokenType
);
