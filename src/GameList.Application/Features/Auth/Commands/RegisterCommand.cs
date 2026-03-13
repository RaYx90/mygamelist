using GameList.Application.Features.Auth.DTOs;
using MediatR;

namespace GameList.Application.Features.Auth.Commands;

/// <summary>
/// Comando MediatR para registrar un nuevo usuario en el sistema.
/// </summary>
/// <param name="Username">Nombre de usuario deseado.</param>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="Password">Contraseña en texto plano.</param>
public sealed record RegisterCommand(string Username, string Email, string Password)
    : IRequest<UserDto>;
