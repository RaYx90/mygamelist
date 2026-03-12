using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Ports;
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
///   <item>Agrupa los favoritos por gameId — cada entrada representa un juego que al menos un miembro desea.</item>
///   <item>Para cada juego: recopila los usernames distintos de quién lo quiere (<c>WantedBy</c>)
///         y quién ya lo ha comprado (<c>PurchasedBy</c>).</item>
///   <item>Usa el primer favorito con la propiedad de navegación <c>Game</c> cargada
///         para extraer los metadatos de presentación (nombre, portada, fecha de lanzamiento más temprana).</item>
///   <item>Ordena de forma descendente por <c>WantedBy.Count</c> para mostrar primero los más populares.</item>
/// </list>
/// </remarks>
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
        if (user?.GroupId is null) return []; // El usuario no pertenece a ningún grupo.

        var members = await _userRepository.GetByGroupIdAsync(user.GroupId.Value, cancellationToken);
        var memberIds = members.Select(m => m.Id).ToList();

        // Diccionario id→username para resolver nombres sin consultas adicionales.
        var usernameById = members.ToDictionary(m => m.Id, m => m.Username);

        // Carga todos los favoritos y compras del grupo en solo dos consultas.
        var allFavs = await _favoriteRepository.GetByUserIdsAsync(memberIds, cancellationToken);
        var allPurchases = await _purchaseRepository.GetByUserIdsAsync(memberIds, cancellationToken);

        // Indexado por gameId para búsqueda O(1) al construir cada entrada de insight.
        var favsByGame = allFavs.GroupBy(f => f.GameId).ToDictionary(g => g.Key, g => g.ToList());
        var purchasesByGame = allPurchases.GroupBy(p => p.GameId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<GroupInsightDto>();
        foreach (var (gameId, favList) in favsByGame)
        {
            // Usernames distintos de los miembros que tienen este juego como favorito.
            var wantedBy = favList.Select(f => usernameById.GetValueOrDefault(f.UserId, "?")).Distinct().ToList();

            // Usernames distintos de los miembros que ya han comprado este juego.
            var purchasedBy = purchasesByGame.TryGetValue(gameId, out var purcs)
                ? purcs.Select(p => usernameById.GetValueOrDefault(p.UserId, "?")).Distinct().ToList()
                : (List<string>)[];

            // Usa el primer favorito con la propiedad Game cargada para obtener los metadatos de presentación.
            // El repositorio carga Game con su colección Releases mediante eager loading.
            var firstFav = favList.FirstOrDefault(f => f.Game is not null);

            // Fecha de lanzamiento más temprana entre todas las plataformas.
            var releaseDate = firstFav?.Game?.Releases.OrderBy(r => r.ReleaseDate).FirstOrDefault()?.ReleaseDate;

            result.Add(new GroupInsightDto(
                GameId: gameId,
                GameName: firstFav?.Game?.Name ?? $"Juego #{gameId}",
                CoverImageUrl: firstFav?.Game?.CoverImageUrl,
                ReleaseDate: releaseDate,
                WantedBy: wantedBy,
                PurchasedBy: purchasedBy));
        }

        // Los juegos más populares (más miembros que los desean) aparecen primero.
        return result.OrderByDescending(r => r.WantedBy.Count).ToList().AsReadOnly();
    }
}
