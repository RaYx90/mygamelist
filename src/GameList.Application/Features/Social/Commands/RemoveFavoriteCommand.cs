using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record RemoveFavoriteCommand(int UserId, int GameId) : IRequest<bool>;
