using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameList.Infrastructure.Clients.LibreTranslate;

internal sealed class LibreTranslateAdapter : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly LibreTranslateOptionsConfig _options;
    private readonly ILogger<LibreTranslateAdapter> _logger;

    public LibreTranslateAdapter(
        HttpClient httpClient,
        IOptions<LibreTranslateOptionsConfig> options,
        ILogger<LibreTranslateAdapter> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<string?>> TranslateBatchAsync(
        IReadOnlyList<string> texts,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        var results = new string?[texts.Count];

        for (int i = 0; i < texts.Count; i++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    $"{_options.ApiUrl}/translate",
                    new
                    {
                        q = texts[i],
                        source = "en",
                        target = targetLanguage.ToLowerInvariant(),
                        format = "text"
                    },
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var body = await response.Content
                    .ReadFromJsonAsync<LibreTranslateResponse>(cancellationToken: cancellationToken);

                results[i] = body?.TranslatedText;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "LibreTranslate failed for text at index {Index}. Skipping.", i);
            }
        }

        return results;
    }

    private sealed record LibreTranslateResponse(
        [property: JsonPropertyName("translatedText")] string TranslatedText);
}
