using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Handler MediatR que devuelve el estado de favoritos y compras del usuario y de los miembros de su grupo.
/// </summary>
public sealed class GetUserGameStatusHandler : IRequestHandler<GetUserGameStatusQuery, UserGameStatusDto>
{
    private readonly IUserRepository userRepository;
    private readonly IGameFavoriteRepository favoriteRepository;
    private readonly IGamePurchaseRepository purchaseRepository;

    /// <summary>
    /// Inicializa el handler con los repositorios necesarios.
    /// </summary>
    /// <param name="userRepository">Repositorio de usuarios.</param>
    /// <param name="favoriteRepository">Repositorio de favoritos.</param>
    /// <param name="purchaseRepository">Repositorio de compras.</param>
    public GetUserGameStatusHandler(IUserRepository userRepository, IGameFavoriteRepository favoriteRepository, IGamePurchaseRepository purchaseRepository)
    {
        this.userRepository = userRepository;
        this.favoriteRepository = favoriteRepository;
        this.purchaseRepository = purchaseRepository;
    }

    /// <summary>
    /// Obtiene los favoritos y compras del usuario y, si pertenece a un grupo, también los de sus compañeros.
    /// </summary>
    /// <param name="request">Consulta con el usuario y los juegos a consultar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO con el estado del usuario y del grupo.</returns>
    public async Task<UserGameStatusDto> Handle(GetUserGameStatusQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null) return Empty();

        var myFavs = await favoriteRepository.GetByUserIdsAndGameIdsAsync([request.UserId], request.GameIds, cancellationToken);
        var myPurchases = await purchaseRepository.GetByUserIdsAndGameIdsAsync([request.UserId], request.GameIds, cancellationToken);

        var myFavsList = myFavs.Select(f => f.GameId).Distinct().ToList();
        var myPurchasesList = myPurchases.Select(p => p.GameId).Distinct().ToList();

        var purchasedByInGroup = new Dictionary<int, IReadOnlyList<string>>();
        var favCountInGroup = new Dictionary<int, int>();

        if (user.GroupId.HasValue)
        {
            var members = await userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
            var memberIds = members.Where(m => m.Id != request.UserId).Select(m => m.Id).ToList();
            if (memberIds.Count > 0)
            {
                var usernameById = members.ToDictionary(m => m.Id, m => m.Username);
                var groupPurchases = await purchaseRepository.GetByUserIdsAndGameIdsAsync(memberIds, request.GameIds, cancellationToken);
                foreach (var g in groupPurchases.GroupBy(p => p.GameId))
                    purchasedByInGroup[g.Key] = g.Select(p => usernameById.GetValueOrDefault(p.UserId, "?")).ToList();
                var groupFavs = await favoriteRepository.GetByUserIdsAndGameIdsAsync(memberIds, request.GameIds, cancellationToken);
                foreach (var g in groupFavs.GroupBy(f => f.GameId))
                    favCountInGroup[g.Key] = g.Count();
            }
        }

        return new UserGameStatusDto(myFavsList, myPurchasesList, purchasedByInGroup, favCountInGroup);
    }

    private static UserGameStatusDto Empty() =>
        new([], [], new Dictionary<int, IReadOnlyList<string>>(), new Dictionary<int, int>());
}
