using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Handler MediatR que añade un juego a favoritos de un usuario de forma idempotente.
/// </summary>
public sealed class AddFavoriteHandler : IRequestHandler<AddFavoriteCommand, bool>
{
    private readonly IGameFavoriteRepository favoriteRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de favoritos.
    /// </summary>
    /// <param name="favoriteRepository">Repositorio de favoritos.</param>
    public AddFavoriteHandler(IGameFavoriteRepository favoriteRepository) => this.favoriteRepository = favoriteRepository;

    /// <summary>
    /// Añade el juego a favoritos si no estaba ya registrado. Devuelve <c>true</c> en ambos casos.
    /// </summary>
    /// <param name="request">Comando con el usuario y el juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns><c>true</c> si el favorito existe o se ha creado correctamente.</returns>
    public async Task<bool> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existing = await favoriteRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is not null) return true;
        await favoriteRepository.AddAsync(GameFavoriteEntity.Create(request.UserId, request.GameId), cancellationToken);
        await favoriteRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
