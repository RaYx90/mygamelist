using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameList.Domain.Ports;
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
        if (texts.Count == 0) return [];

        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"{_options.ApiUrl}/translate",
                new
                {
                    q = texts,
                    source = "en",
                    target = targetLanguage.ToLowerInvariant(),
                    format = "text"
                },
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content
                .ReadFromJsonAsync<LibreTranslateBatchResponse>(cancellationToken: cancellationToken);

            if (body?.TranslatedTexts is not null && body.TranslatedTexts.Count == texts.Count)
                return body.TranslatedTexts.Select(t => (string?)t).ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LibreTranslate batch of {Count} texts failed. Skipping.", texts.Count);
        }

        return texts.Select(_ => (string?)null).ToArray();
    }

    private sealed record LibreTranslateBatchResponse(
        [property: JsonPropertyName("translatedText")] List<string> TranslatedTexts);
}
