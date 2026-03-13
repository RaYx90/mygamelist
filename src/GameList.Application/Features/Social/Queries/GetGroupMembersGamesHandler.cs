using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Maneja <see cref="GetGroupMembersGamesQuery"/>: devuelve los favoritos y compras de cada miembro
/// del grupo como una vista por miembro, ordenada alfabéticamente por username.
/// </summary>
/// <remarks>
/// Este handler produce una vista centrada en el miembro (una entrada por usuario), mientras que
/// <see cref="GetGroupInsightsHandler"/> produce una vista centrada en el juego (una entrada por juego).
/// Ambos handlers cargan favoritos y compras en dos consultas batch para evitar el problema N+1.
/// </remarks>
public sealed class GetGroupMembersGamesHandler : IRequestHandler<GetGroupMembersGamesQuery, IReadOnlyList<MemberGamesDto>>
{
    private readonly IUserRepository userRepository;
    private readonly IGameFavoriteRepository favoriteRepository;
    private readonly IGamePurchaseRepository purchaseRepository;

    public GetGroupMembersGamesHandler(
        IUserRepository userRepository,
        IGameFavoriteRepository favoriteRepository,
        IGamePurchaseRepository purchaseRepository)
    {
        this.userRepository = userRepository;
        this.favoriteRepository = favoriteRepository;
        this.purchaseRepository = purchaseRepository;
    }

    public async Task<IReadOnlyList<MemberGamesDto>> Handle(GetGroupMembersGamesQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user?.GroupId is null) return []; // El usuario no pertenece a ningún grupo.

        var members = await userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberIds = members.Select(m => m.Id).ToList();

        // Carga favoritos y compras del grupo en dos consultas batch.
        var allFavs = await favoriteRepository.GetByUserIdsAsync(memberIds, cancellationToken);
        var allPurchases = await purchaseRepository.GetByUserIdsAsync(memberIds, cancellationToken);

        // Indexado por userId para resolución O(1) de las listas de cada miembro.
        var favsByUser = allFavs.GroupBy(f => f.UserId).ToDictionary(g => g.Key, g => g.ToList());
        var purchasesByUser = allPurchases.GroupBy(p => p.UserId).ToDictionary(g => g.Key, g => g.ToList());

        return members
            .OrderBy(m => m.Username)
            .Select(m => new MemberGamesDto(
                Username: m.Username,
                Favorites: favsByUser.TryGetValue(m.Id, out var favs)
                    ? favs
                        // Protección por si la propiedad de navegación Game no fue cargada.
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
