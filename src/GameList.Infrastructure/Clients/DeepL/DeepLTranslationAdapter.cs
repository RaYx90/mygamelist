using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameList.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameList.Infrastructure.Clients.DeepL;

internal sealed class DeepLTranslationAdapter : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly DeepLOptionsConfig _options;
    private readonly ILogger<DeepLTranslationAdapter> _logger;

    private const int BatchSize = 50;

    public DeepLTranslationAdapter(
        HttpClient httpClient,
        IOptions<DeepLOptionsConfig> options,
        ILogger<DeepLTranslationAdapter> logger)
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
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogDebug("DeepL API key not configured — skipping translation.");
            return texts.Select(_ => (string?)null).ToArray();
        }

        var results = new string?[texts.Count];

        for (int i = 0; i < texts.Count; i += BatchSize)
        {
            var chunk = texts.Skip(i).Take(BatchSize).ToArray();

            try
            {
                var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"{_options.ApiUrl}/v2/translate");

                request.Headers.Authorization =
                    new AuthenticationHeaderValue("DeepL-Auth-Key", _options.ApiKey);

                request.Content = JsonContent.Create(new
                {
                    text = chunk,
                    target_lang = targetLanguage.ToUpperInvariant()
                });

                var response = await _httpClient.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();

                var body = await response.Content
                    .ReadFromJsonAsync<DeepLResponse>(cancellationToken: cancellationToken);

                if (body?.Translations is not null)
                {
                    for (int j = 0; j < body.Translations.Count; j++)
                        results[i + j] = body.Translations[j].Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "DeepL translation failed for batch starting at index {Index}. Skipping.", i);
            }
        }

        return results;
    }

    private sealed record DeepLResponse(
        [property: JsonPropertyName("translations")] List<DeepLTranslation> Translations);

    private sealed record DeepLTranslation(
        [property: JsonPropertyName("text")] string Text);
}
