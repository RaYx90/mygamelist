namespace GameList.Application.Features.Social.DTOs;

public sealed record GroupInsightDto(
    int GameId,
    string GameName,
    string? CoverImageUrl,
    DateOnly? ReleaseDate,
    IReadOnlyList<string> WantedBy,
    IReadOnlyList<string> PurchasedBy
);
