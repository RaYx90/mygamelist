namespace GameList.Application.Features.Social.DTOs;

public sealed record MemberGamesDto(
    string Username,
    IReadOnlyList<GameSummaryDto> Favorites,
    IReadOnlyList<GameSummaryDto> Purchases
);
