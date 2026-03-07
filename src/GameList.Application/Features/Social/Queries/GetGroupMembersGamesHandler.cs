using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

public sealed class GetGroupMembersGamesHandler : IRequestHandler<GetGroupMembersGamesQuery, IReadOnlyList<MemberGamesDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IGameFavoriteRepository _favoriteRepository;
    private readonly IGamePurchaseRepository _purchaseRepository;

    public GetGroupMembersGamesHandler(
        IUserRepository userRepository,
        IGameFavoriteRepository favoriteRepository,
        IGamePurchaseRepository purchaseRepository)
    {
        _userRepository = userRepository;
        _favoriteRepository = favoriteRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<IReadOnlyList<MemberGamesDto>> Handle(GetGroupMembersGamesQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user?.GroupId is null) return [];

        var members = await _userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberIds = members.Select(m => m.Id).ToList();

        var allFavs = await _favoriteRepository.GetByUserIdsAsync(memberIds, cancellationToken);
        var allPurchases = await _purchaseRepository.GetByUserIdsAsync(memberIds, cancellationToken);

        var favsByUser = allFavs.GroupBy(f => f.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var purchasesByUser = allPurchases.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.ToList());

        return members
            .OrderBy(m => m.Username)
            .Select(m => new MemberGamesDto(
                Username: m.Username,
                Favorites: favsByUser.TryGetValue(m.Id, out var favs)
                    ? favs
                        .Where(f => f.Game is not null)
                        .Select(f => new GameSummaryDto(f.GameId, f.Game!.Name, f.Game.CoverImageUrl))
                        .ToList()
                    : [],
                Purchases: purchasesByUser.TryGetValue(m.Id, out var purcs)
                    ? purcs
                        .Where(p => p.Game is not null)
                        .Select(p => new GameSummaryDto(p.GameId, p.Game!.Name, p.Game.CoverImageUrl))
                        .ToList()
                    : []
            ))
            .ToList()
            .AsReadOnly();
    }
}
