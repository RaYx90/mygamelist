using GameList.Domain.Entities;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class MarkPurchasedHandler : IRequestHandler<MarkPurchasedCommand, bool>
{
    private readonly IGamePurchaseRepository _purchaseRepository;
    public MarkPurchasedHandler(IGamePurchaseRepository purchaseRepository) => _purchaseRepository = purchaseRepository;

    public async Task<bool> Handle(MarkPurchasedCommand request, CancellationToken cancellationToken)
    {
        var existing = await _purchaseRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is not null) return true;
        await _purchaseRepository.AddAsync(GamePurchaseEntity.Create(request.UserId, request.GameId), cancellationToken);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
