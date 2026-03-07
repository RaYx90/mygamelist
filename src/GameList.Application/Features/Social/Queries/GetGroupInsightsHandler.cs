using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

public sealed class GetGroupInsightsHandler : IRequestHandler<GetGroupInsightsQuery, IReadOnlyList<GroupInsightDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IGameFavoriteRepository _favoriteRepository;
    private readonly IGamePurchaseRepository _purchaseRepository;

    public GetGroupInsightsHandler(IUserRepository userRepository, IGameFavoriteRepository favoriteRepository, IGamePurchaseRepository purchaseRepository)
    {
        _userRepository = userRepository;
        _favoriteRepository = favoriteRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<IReadOnlyList<GroupInsightDto>> Handle(GetGroupInsightsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user?.GroupId is null) return [];

        var members = await _userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberIds = members.Select(m => m.Id).ToList();
        var usernameById = members.ToDictionary(m => m.Id, m => m.Username);

        var allFavs = await _favoriteRepository.GetByUserIdsAsync(memberIds, cancellationToken);
        var allPurchases = await _purchaseRepository.GetByUserIdsAsync(memberIds, cancellationToken);

        var favsByGame = allFavs.GroupBy(f => f.GameId).ToDictionary(g => g.Key, g => g.ToList());
        var purchasesByGame = allPurchases.GroupBy(p => p.GameId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<GroupInsightDto>();
        foreach (var (gameId, favList) in favsByGame)
        {
            var wantedBy = favList.Select(f => usernameById.GetValueOrDefault(f.UserId, "?")).Distinct().ToList();
            var purchasedBy = purchasesByGame.TryGetValue(gameId, out var purcs)
                ? purcs.Select(p => usernameById.GetValueOrDefault(p.UserId, "?")).Distinct().ToList()
                : (List<string>)[];

            var firstFav = favList.FirstOrDefault(f => f.Game is not null);
            var releaseDate = firstFav?.Game?.Releases.OrderBy(r => r.ReleaseDate).FirstOrDefault()?.ReleaseDate;

            result.Add(new GroupInsightDto(
                GameId: gameId,
                GameName: firstFav?.Game?.Name ?? $"Juego #{gameId}",
                CoverImageUrl: firstFav?.Game?.CoverImageUrl,
                ReleaseDate: releaseDate,
                WantedBy: wantedBy,
                PurchasedBy: purchasedBy));
        }

        return result.OrderByDescending(r => r.WantedBy.Count).ToList().AsReadOnly();
    }
}
