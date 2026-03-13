using MediatR;
namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Comando MediatR para desmarcar un juego como comprado por un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="GameId">Identificador del juego a desmarcar.</param>
public sealed record UnmarkPurchasedCommand(int UserId, int GameId) : IRequest<bool>;
