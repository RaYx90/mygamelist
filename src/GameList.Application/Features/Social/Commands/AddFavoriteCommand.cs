using MediatR;
namespace GameList.Application.Features.Social.Commands;
public sealed record AddFavoriteCommand(int UserId, int GameId) : IRequest<bool>;
