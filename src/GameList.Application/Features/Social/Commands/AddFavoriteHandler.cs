using GameList.Domain.Entities;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

public sealed class AddFavoriteHandler : IRequestHandler<AddFavoriteCommand, bool>
{
    private readonly IGameFavoriteRepository _favoriteRepository;
    public AddFavoriteHandler(IGameFavoriteRepository favoriteRepository) => _favoriteRepository = favoriteRepository;

    public async Task<bool> Handle(AddFavoriteCommand request, CancellationToken cancellationToken)
    {
        var existing = await _favoriteRepository.GetAsync(request.UserId, request.GameId, cancellationToken);
        if (existing is not null) return true;
        await _favoriteRepository.AddAsync(GameFavoriteEntity.Create(request.UserId, request.GameId), cancellationToken);
        await _favoriteRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
