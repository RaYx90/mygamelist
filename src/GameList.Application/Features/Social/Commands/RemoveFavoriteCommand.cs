using MediatR;
namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Comando MediatR para eliminar un juego de la lista de favoritos de un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="GameId">Identificador del juego a eliminar de favoritos.</param>
public sealed record RemoveFavoriteCommand(int UserId, int GameId) : IRequest<bool>;
