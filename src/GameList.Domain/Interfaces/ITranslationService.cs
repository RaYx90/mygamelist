namespace GameList.Domain.Interfaces;

/// <summary>
/// Translates a batch of texts to the target language.
/// Returns null entries for texts that could not be translated.
/// </summary>
public interface ITranslationService
{
    Task<IReadOnlyList<string?>> TranslateBatchAsync(
        IReadOnlyList<string> texts,
        string targetLanguage,
        CancellationToken cancellationToken = default);
}
