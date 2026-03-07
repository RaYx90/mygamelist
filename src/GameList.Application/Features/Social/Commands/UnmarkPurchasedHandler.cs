using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class UnmarkPurchasedHandler : IRequestHandler<UnmarkPurchasedCommand, bool>
{
    private readonly IGamePurchaseRepository _purchaseRepository;
    public UnmarkPurchasedHandler(IGamePurchaseRepository purchaseRepository) => _purchaseRepository = purchaseRepository;

    public async Task<bool> Handle(UnmarkPurchasedCommand request, CancellationToken cancellationToken)
    {
        var existing = await _purchaseRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is null) return false;
        _purchaseRepository.Remove(existing);
        await _purchaseRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
