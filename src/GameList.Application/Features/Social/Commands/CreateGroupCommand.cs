using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record CreateGroupCommand(int UserId, string GroupName) : IRequest<GroupDto>;
