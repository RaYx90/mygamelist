using GameList.Application.Features.Games.DTOs;
using MediatR;

namespace GameList.Application.Features.Games.Queries;

public sealed record GetGameDetailQuery(int GameId) : IRequest<GameDetailDto>;
