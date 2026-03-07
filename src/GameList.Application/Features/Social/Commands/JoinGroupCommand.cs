using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record JoinGroupCommand(int UserId, string InviteCode) : IRequest<bool>;
