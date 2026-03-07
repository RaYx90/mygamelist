using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

public sealed class GetUserGameStatusHandler : IRequestHandler<GetUserGameStatusQuery, UserGameStatusDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IGameFavoriteRepository _favoriteRepository;
    private readonly IGamePurchaseRepository _purchaseRepository;

    public GetUserGameStatusHandler(IUserRepository userRepository, IGameFavoriteRepository favoriteRepository, IGamePurchaseRepository purchaseRepository)
    {
        _userRepository = userRepository;
        _favoriteRepository = favoriteRepository;
        _purchaseRepository = purchaseRepository;
    }

    public async Task<UserGameStatusDto> Handle(GetUserGameStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Empty();

        var myFavs = await _favoriteRepository.GetByUserIdsAndGameIdsAsync([request.UserId], request.GameIds, cancellationToken);
        var myPurchases = await _purchaseRepository.GetByUserIdsAndGameIdsAsync([request.UserId], request.GameIds, cancellationToken);

        var myFavsList = myFavs.Select(f => f.GameId).Distinct().ToList();
        var myPurchasesList = myPurchases.Select(p => p.GameId).Distinct().ToList();

        var purchasedByInGroup = new Dictionary<int, IReadOnlyList<string>>();
        var favCountInGroup = new Dictionary<int, int>();

        if (user.GroupId.HasValue)
        {
            var members = await _userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
            var memberIds = members.Where(m => m.Id != request.UserId).Select(m => m.Id).ToList();
            if (memberIds.Count > 0)
            {
                var usernameById = members.ToDictionary(m => m.Id, m => m.Username);
                var groupPurchases = await _purchaseRepository.GetByUserIdsAndGameIdsAsync(memberIds, request.GameIds, cancellationToken);
                foreach (var g in groupPurchases.GroupBy(p => p.GameId))
                    purchasedByInGroup[g.Key] = g.Select(p => usernameById.GetValueOrDefault(p.UserId, "?")).ToList();
                var groupFavs = await _favoriteRepository.GetByUserIdsAndGameIdsAsync(memberIds, request.GameIds, cancellationToken);
                foreach (var g in groupFavs.GroupBy(f => f.GameId))
                    favCountInGroup[g.Key] = g.Count();
            }
        }

        return new UserGameStatusDto(myFavsList, myPurchasesList, purchasedByInGroup, favCountInGroup);
    }

    private static UserGameStatusDto Empty() =>
        new([], [], new Dictionary<int, IReadOnlyList<string>>(), new Dictionary<int, int>());
}
