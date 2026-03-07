using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class RemoveFavoriteHandler : IRequestHandler<RemoveFavoriteCommand, bool>
{
    private readonly IGameFavoriteRepository _favoriteRepository;
    public RemoveFavoriteHandler(IGameFavoriteRepository favoriteRepository) => _favoriteRepository = favoriteRepository;

    public async Task<bool> Handle(RemoveFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existing = await _favoriteRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is null) return false;
        _favoriteRepository.Remove(existing);
        await _favoriteRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
