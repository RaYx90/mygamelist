namespace GameList.Application.Features.Social.DTOs;

public sealed record GameSummaryDto(
    int GameId,
    string GameName,
    string? CoverImageUrl
);
