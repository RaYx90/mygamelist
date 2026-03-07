using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Queries;
public sealed record GetMyGroupQuery(int UserId) : IRequest<GroupDto?>;
