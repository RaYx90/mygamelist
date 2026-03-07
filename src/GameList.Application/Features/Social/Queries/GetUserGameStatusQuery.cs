using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Queries;
public sealed record GetUserGameStatusQuery(int UserId, IReadOnlyList<int> GameIds) : IRequest<UserGameStatusDto>;
