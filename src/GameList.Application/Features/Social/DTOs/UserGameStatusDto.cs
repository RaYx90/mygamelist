namespace GameList.Application.Features.Social.DTOs;

public sealed record UserGameStatusDto(
    IReadOnlyList<int> MyFavorites,
    IReadOnlyList<int> MyPurchases,
    IReadOnlyDictionary<int, IReadOnlyList<string>> PurchasedByInGroup,
    IReadOnlyDictionary<int, int> FavoritedCountInGroup
);
