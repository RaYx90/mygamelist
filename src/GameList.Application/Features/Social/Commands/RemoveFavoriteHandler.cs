using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Handler MediatR que elimina un juego de favoritos de un usuario.
/// </summary>
public sealed class RemoveFavoriteHandler : IRequestHandler<RemoveFavoriteCommand, bool>
{
    private readonly IGameFavoriteRepository favoriteRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de favoritos.
    /// </summary>
    /// <param name="favoriteRepository">Repositorio de favoritos.</param>
    public RemoveFavoriteHandler(IGameFavoriteRepository favoriteRepository) => this.favoriteRepository = favoriteRepository;

    /// <summary>
    /// Elimina el favorito si existe. Devuelve <c>false</c> si no estaba registrado.
    /// </summary>
    /// <param name="request">Comando con el usuario y el juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si se eliminó correctamente; <c>false</c> si no existía.</returns>
    public async Task<bool> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existing = await favoriteRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is null) return false;
        favoriteRepository.Remove(existing);
        await favoriteRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
