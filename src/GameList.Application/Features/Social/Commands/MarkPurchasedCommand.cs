using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record MarkPurchasedCommand(int UserId, int GameId) : IRequest<bool>;
