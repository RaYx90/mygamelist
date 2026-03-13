using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Maneja <see cref="GetGroupInsightsQuery"/>: agrega los favoritos y compras de todos los miembros
/// del grupo en una lista de juegos ordenada por popularidad (más deseados primero).
/// </summary>
/// <remarks>
/// Algoritmo de agregación:
/// <list type="number">
///   <item>Carga todos los favoritos y compras del grupo en dos consultas batch (evita N+1).</item>
///   <item>Recorre todos los juegos que aparecen en favoritos O compras del grupo.</item>
///   <item>Filtra: solo es coincidencia si más de 1 persona lo desea, más de 1 lo ha comprado,
///         o al menos 1 lo desea Y al menos 1 lo ha comprado.</item>
///   <item>Ordena descendente por <c>WantedBy.Count</c> y luego por <c>PurchasedBy.Count</c>.</item>
/// </list>
/// </remarks>
public sealed class GetGroupInsightsHandler : IRequestHandler<GetGroupInsightsQuery, IReadOnlyList<GroupInsightDto>>
{
    private readonly IUserRepository userRepository;
    private readonly IGameFavoriteRepository favoriteRepository;
    private readonly IGamePurchaseRepository purchaseRepository;

    public GetGroupInsightsHandler(IUserRepository userRepository, IGameFavoriteRepository favoriteRepository, IGamePurchaseRepository purchaseRepository)
    {
        this.userRepository = userRepository;
        this.favoriteRepository = favoriteRepository;
        this.purchaseRepository = purchaseRepository;
    }

    public async Task<IReadOnlyList<GroupInsightDto>> Handle(GetGroupInsightsQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user?.GroupId is null) return []; // El usuario no pertenece a ningún grupo.

        var members = await userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberIds = members.Select(m => m.Id).ToList();

        // Diccionario id→username para resolver nombres sin consultas adicionales.
        // GroupBy evita excepción por claves duplicadas (entidades sin persistir tienen Id=0 en tests).
        var usernameById = members.GroupBy(m => m.Id).ToDictionary(g => g.Key, g => g.First().Username);

        // Carga todos los favoritos y compras del grupo en solo dos consultas.
        var allFavs = await favoriteRepository.GetByUserIdsAsync(memberIds, cancellationToken);
        var allPurchases = await purchaseRepository.GetByUserIdsAsync(memberIds, cancellationToken);

        // Indexado por gameId para búsqueda O(1) al construir cada entrada de insight.
        var favsByGame = allFavs.GroupBy(f => f.GameId).ToDictionary(g => g.Key, g => g.ToList());
        var purchasesByGame = allPurchases.GroupBy(p => p.GameId).ToDictionary(g => g.Key, g => g.ToList());

        // Recorre todos los juegos que aparecen en favoritos O en compras del grupo.
        var allGameIds = favsByGame.Keys.Union(purchasesByGame.Keys);

        var result = new List<GroupInsightDto>();
        foreach (var gameId in allGameIds)
        {
            var wantedBy = favsByGame.TryGetValue(gameId, out var favList)
                ? favList.Select(f => usernameById.GetValueOrDefault(f.UserId, "?")).Distinct().ToList()
                : (List<string>)[];

            var purchasedBy = purchasesByGame.TryGetValue(gameId, out var purcs)
                ? purcs.Select(p => usernameById.GetValueOrDefault(p.UserId, "?")).Distinct().ToList()
                : (List<string>)[];

            // Coincidencia: más de 1 persona lo desea, más de 1 lo ha comprado,
            // o al menos 1 lo desea Y al menos 1 lo ha comprado.
            bool isCoincidence = wantedBy.Count > 1
                || purchasedBy.Count > 1
                || (wantedBy.Count >= 1 && purchasedBy.Count >= 1);

            if (!isCoincidence) continue;

            // Metadatos del juego: favoritos tienen Releases cargados; compras también tras el fix del repositorio.
            var game = favList?.FirstOrDefault(f => f.Game is not null)?.Game
                    ?? purcs?.FirstOrDefault(p => p.Game is not null)?.Game;

            var releaseDate = game?.Releases.OrderBy(r => r.ReleaseDate).FirstOrDefault()?.ReleaseDate;
            var coverUrl = game?.CoverImageUrl?.Replace("/t_thumb/", "/t_cover_big/");

            result.Add(new GroupInsightDto(
                GameId: gameId,
                GameName: game?.Name ?? $"Juego #{gameId}",
                CoverImageUrl: coverUrl,
                ReleaseDate: releaseDate,
                WantedBy: wantedBy,
                PurchasedBy: purchasedBy));
        }

        // Los juegos con más miembros que los desean aparecen primero.
        return result.OrderByDescending(r => r.WantedBy.Count).ThenByDescending(r => r.PurchasedBy.Count).ToList().AsReadOnly();
    }
}
