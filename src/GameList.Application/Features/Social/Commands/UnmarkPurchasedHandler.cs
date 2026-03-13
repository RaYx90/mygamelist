using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Handler MediatR que elimina el registro de compra de un juego para un usuario.
/// </summary>
public sealed class UnmarkPurchasedHandler : IRequestHandler<UnmarkPurchasedCommand, bool>
{
    private readonly IGamePurchaseRepository purchaseRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de compras.
    /// </summary>
    /// <param name="purchaseRepository">Repositorio de compras.</param>
    public UnmarkPurchasedHandler(IGamePurchaseRepository purchaseRepository) => this.purchaseRepository = purchaseRepository;

    /// <summary>
    /// Elimina el registro de compra si existe. Devuelve <c>false</c> si no estaba registrado.
    /// </summary>
    /// <param name="request">Comando con el usuario y el juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si se eliminó correctamente; <c>false</c> si no existía.</returns>
    public async Task<bool> Handle(UnmarkPurchasedCommand request, CancellationToken cancellationToken)
    {
        var existing = await purchaseRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is null) return false;
        purchaseRepository.Remove(existing);
        await purchaseRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
