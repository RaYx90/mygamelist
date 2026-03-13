namespace GameList.Application.Features.Social.DTOs;

/// <summary>
/// DTO con los juegos favoritos y comprados de un miembro de un grupo social.
/// </summary>
/// <param name="Username">Nombre de usuario del miembro.</param>
/// <param name="Favorites">Juegos marcados como favoritos por el miembro.</param>
/// <param name="Purchases">Juegos marcados como comprados por el miembro.</param>
public sealed record MemberGamesDto(
    string Username,
    IReadOnlyList<GameSummaryDto> Favorites,
    IReadOnlyList<GameSummaryDto> Purchases
);
