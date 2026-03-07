using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record UnmarkPurchasedCommand(int UserId, int GameId) : IRequest<bool>;
