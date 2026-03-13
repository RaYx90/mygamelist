using MediatR;
namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Comando MediatR para añadir un juego a la lista de favoritos de un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="GameId">Identificador del juego a añadir como favorito.</param>
public sealed record AddFavoriteCommand(int UserId, int GameId) : IRequest<bool>;
