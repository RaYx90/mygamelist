using GameList.Application.Common.Mappers;
using GameList.Application.Features.Games.DTOs;
using GameList.Domain.Exceptions;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Games.Queries;

/// <summary>
/// Handler MediatR que devuelve el detalle de un juego o lanza <see cref="GameNotFoundException"/> si no existe.
/// </summary>
public sealed class GetGameDetailHandler
    : IRequestHandler<GetGameDetailQuery, GameDetailDto>
{
    private readonly IGameRepository gameRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de juegos.
    /// </summary>
    /// <param name="gameRepository">Repositorio de juegos.</param>
    public GetGameDetailHandler(IGameRepository gameRepository)
    {
        this.gameRepository = gameRepository;
    }

    /// <summary>
    /// Obtiene el juego por su identificador y lo convierte al DTO de detalle.
    /// </summary>
    /// <param name="request">Consulta con el identificador del juego.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>DTO con el detalle del juego.</returns>
    public async Task<GameDetailDto> Handle(
        GetGameDetailQuery request,
        CancellationToken cancellationToken)
    {
        var game = await gameRepository.GetByIdAsync(request.GameId, cancellationToken)
            ?? throw new GameNotFoundException(request.GameId);

        return GameMapper.ToDetailDto(game);
    }
}
