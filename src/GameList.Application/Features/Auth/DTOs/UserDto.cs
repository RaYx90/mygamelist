namespace GameList.Application.Features.Auth.DTOs;

/// <summary>
/// DTO con los datos básicos de un usuario autenticado.
/// </summary>
/// <param name="UserId">Identificador interno del usuario.</param>
/// <param name="Username">Nombre de usuario.</param>
/// <param name="Email">Correo electrónico del usuario.</param>
/// <param name="GroupId">Identificador del grupo al que pertenece el usuario (opcional).</param>
/// <param name="AvatarPath">Ruta relativa al avatar del usuario (opcional).</param>
public sealed record UserDto(int UserId, string Username, string Email, int? GroupId, string? AvatarPath);
