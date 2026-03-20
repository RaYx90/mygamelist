using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>Devuelve la lista completa de favoritos del usuario autenticado.</summary>
public sealed class GetMyFavoritesHandler : IRequestHandler<GetMyFavoritesQuery, IReadOnlyList<GameSummaryDto>>
{
    private readonly IGameFavoriteRepository favoriteRepository;

    public GetMyFavoritesHandler(IGameFavoriteRepository favoriteRepository)
        => this.favoriteRepository = favoriteRepository;

    public async Task<IReadOnlyList<GameSummaryDto>> Handle(GetMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        // GetByUserIdsAsync incluye la navegación a Game; GetByUserIdAsync no la incluye.
        var favorites = await favoriteRepository.GetByUserIdsAsync([request.UserId], cancellationToken);
        return favorites
            .Where(f => f.Game is not null)
            .OrderBy(f => f.Game!.Name)
            .Select(f => new GameSummaryDto(
                f.GameId,
                f.Game!.Name,
                f.Game.CoverImageUrl,
                f.Game.Releases.Any() ? f.Game.Releases.Min(r => r.ReleaseDate) : (DateOnly?)null))
            .ToList()
            .AsReadOnly();
    }
}
