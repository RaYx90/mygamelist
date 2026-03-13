namespace GameList.Application.Features.Social.DTOs;

/// <summary>
/// DTO con el estado de favoritos y compras de un usuario para un conjunto de juegos,
/// incluyendo información de los miembros de su grupo.
/// </summary>
/// <param name="MyFavorites">IDs de juegos marcados como favoritos por el usuario.</param>
/// <param name="MyPurchases">IDs de juegos marcados como comprados por el usuario.</param>
/// <param name="PurchasedByInGroup">Diccionario de ID de juego a lista de nombres de miembros del grupo que lo han comprado.</param>
/// <param name="FavoritedCountInGroup">Diccionario de ID de juego al número de miembros del grupo que lo tienen como favorito.</param>
public sealed record UserGameStatusDto(
    IReadOnlyList<int> MyFavorites,
    IReadOnlyList<int> MyPurchases,
    IReadOnlyDictionary<int, IReadOnlyList<string>> PurchasedByInGroup,
    IReadOnlyDictionary<int, int> FavoritedCountInGroup
);
