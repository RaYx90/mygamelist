using MediatR;

namespace GameList.Application.Features.Auth.Commands;

/// <summary>Cambia el nombre de usuario del usuario autenticado.</summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="NewUsername">Nuevo nombre de usuario.</param>
public sealed record ChangeUsernameCommand(int UserId, string NewUsername) : IRequest<string>;
