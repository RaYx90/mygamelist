using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Handler MediatR que marca un juego como comprado por un usuario de forma idempotente.
/// </summary>
public sealed class MarkPurchasedHandler : IRequestHandler<MarkPurchasedCommand, bool>
{
    private readonly IGamePurchaseRepository purchaseRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de compras.
    /// </summary>
    /// <param name="purchaseRepository">Repositorio de compras.</param>
    public MarkPurchasedHandler(IGamePurchaseRepository purchaseRepository) => this.purchaseRepository = purchaseRepository;

    /// <summary>
    /// Registra la compra si no estaba ya registrada. Devuelve <c>true</c> en ambos casos.
    /// </summary>
    /// <param name="request">Comando con el usuario y el juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si la compra existe o se ha registrado correctamente.</returns>
    public async Task<bool> Handle(MarkPurchasedCommand request, CancellationToken cancellationToken)
    {
        var existing = await purchaseRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is not null) return true;
        await purchaseRepository.AddAsync(GamePurchaseEntity.Create(request.UserId, request.GameId), cancellationToken);
        await purchaseRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
