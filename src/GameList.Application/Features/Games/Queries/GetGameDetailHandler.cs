using GameList.Application.Common.Mappers;
using GameList.Application.Features.Games.DTOs;
using GameList.Domain.Exceptions;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Games.Queries;

public sealed class GetGameDetailHandler
    : IRequestHandler<GetGameDetailQuery, GameDetailDto>
{
    private readonly IGameRepository _gameRepository;

    public GetGameDetailHandler(IGameRepository gameRepository)
    {
        _gameRepository = gameRepository;
    }

    public async Task<GameDetailDto> Handle(
        GetGameDetailQuery request,
        CancellationToken cancellationToken)
    {
        var game = await _gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        return GameMapper.ToDetailDto(game);
    }
}
