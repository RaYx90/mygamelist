using GameList.Application.Features.Social.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>Devuelve la lista completa de juegos comprados del usuario autenticado.</summary>
public sealed class GetMyPurchasesHandler : IRequestHandler<GetMyPurchasesQuery, IReadOnlyList<GameSummaryDto>>
{
    private readonly IGamePurchaseRepository purchaseRepository;

    public GetMyPurchasesHandler(IGamePurchaseRepository purchaseRepository)
        => this.purchaseRepository = purchaseRepository;

    public async Task<IReadOnlyList<GameSummaryDto>> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
    {
        var purchases = await purchaseRepository.GetByUserIdsAsync([request.UserId], cancellationToken);
        return purchases
            .Where(p => p.Game is not null)
            .Where(p => p.Game!.Releases.Any()) // Solo juegos con al menos 1 release activo
            .OrderBy(p => p.Game!.Name)
            .Select(p => new GameSummaryDto(
                p.GameId,
                p.Game!.Name,
                p.Game.CoverImageUrl,
                p.Game.Releases.Any() ? p.Game.Releases.Min(r => r.ReleaseDate) : (DateOnly?)null))
            .ToList()
            .AsReadOnly();
    }
}
