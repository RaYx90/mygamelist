using GameList.Application.Features.Auth.DTOs;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

/// <summary>
/// Comando MediatR para autenticar a un usuario con su email y contraseña.
/// </summary>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public sealed record LoginCommand(string Email, string Password) : IRequest<UserDto?>;
