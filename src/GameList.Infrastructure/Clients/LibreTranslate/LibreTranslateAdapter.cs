using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GameList.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameList.Infrastructure.Clients.LibreTranslate;

/// <summary>
/// Adaptador para la API REST de LibreTranslate autoalojado (contenedor Docker).
/// Implementa <see cref="ITranslationService"/> traduciendo lotes de textos en un solo request HTTP.
/// </summary>
/// <remarks>
/// Estrategia de fallos: si LibreTranslate no está disponible o devuelve error,
/// el método devuelve una lista de nulls del mismo tamaño que la entrada (en lugar de lanzar).
/// Esto permite a <see cref="TranslationBackgroundService"/> omitir el lote y reintentar en el siguiente tick.
/// </remarks>
internal sealed class LibreTranslateAdapter : ITranslationService
{
    private readonly HttpClient httpClient;
    private readonly LibreTranslateOptionsConfig options;
    private readonly ILogger<LibreTranslateAdapter> logger;

    public LibreTranslateAdapter(
        HttpClient httpClient,
        IOptions<LibreTranslateOptionsConfig> options,
        ILogger<LibreTranslateAdapter> logger)
    {
        this.httpClient = httpClient;
        this.options = options.Value;
        this.logger = logger;
    }

    /// <summary>
    /// Traduce un lote de textos al idioma indicado usando la API batch de LibreTranslate.
    /// Devuelve una lista del mismo tamaño que <paramref name="texts"/>:
    /// cada posición contiene la traducción o <c>null</c> si falló.
    /// </summary>
    public async Task<IReadOnlyList<string?>> TranslateBatchAsync(
        IReadOnlyList<string> texts,
        string targetLanguage,
        CancellationToken cancellationToken = default)
    {
        if (texts.Count == 0) return [];

        try
        {
            // Contrato de la API de LibreTranslate:
            // POST /translate con body JSON { q: string[], source: "en", target: "es", format: "text" }
            // Respuesta: { translatedText: string[] } — mismo orden que la entrada.
            var response = await httpClient.PostAsJsonAsync(
                $"{options.ApiUrl}/translate",
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

            // Validación: la respuesta debe contener exactamente tantos textos como se enviaron.
            if (body?.TranslatedTexts is not null && body.TranslatedTexts.Count == texts.Count)
                return body.TranslatedTexts.Select(t => (string?)t).ToArray();
        }
        catch (Exception ex)
        {
            // Fallo silencioso — se registra advertencia pero no se propaga la excepción.
            // El servicio de traducción en background reintentará este lote en el siguiente tick.
            logger.LogWarning(ex, "Lote de {Count} textos en LibreTranslate falló. Se omite.", texts.Count);
        }

        // En caso de fallo parcial o total, devuelve nulls para que el caller decida qué hacer.
        return texts.Select(_ => (string?)null).ToArray();
    }

    // Record interno para deserializar la respuesta JSON de LibreTranslate.
    private sealed record LibreTranslateBatchResponse(
        [property: JsonPropertyName("translatedText")] List<string> TranslatedTexts);
}
