using MediatR;
namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Comando MediatR para marcar un juego como comprado por un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="GameId">Identificador del juego a marcar como comprado.</param>
public sealed record MarkPurchasedCommand(int UserId, int GameId) : IRequest<bool>;
