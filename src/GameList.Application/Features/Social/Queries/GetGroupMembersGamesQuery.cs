using GameList.Application.Features.Social.DTOs;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

public sealed record GetGroupMembersGamesQuery(int UserId) : IRequest<IReadOnlyList<MemberGamesDto>>;
