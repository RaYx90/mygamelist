using GameList.Application.Features.Games.DTOs;
using MediatR;

namespace GameList.Application.Features.Games.Queries;

/// <summary>
/// Consulta MediatR para obtener el detalle completo de un juego, incluyendo sus lanzamientos.
/// </summary>
/// <param name="GameId">Identificador del juego a consultar.</param>
public sealed record GetGameDetailQuery(int GameId) : IRequest<GameDetailDto>;
