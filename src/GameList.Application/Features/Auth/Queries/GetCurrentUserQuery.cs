using GameList.Application.Features.Auth.DTOs;
using MediatR;

namespace GameList.Application.Features.Auth.Queries;

/// <summary>
/// Query MediatR que obtiene los datos del usuario actualmente autenticado por su ID.
/// </summary>
/// <param name="UserId">Identificador del usuario extraído del claim JWT.</param>
public sealed record GetCurrentUserQuery(int UserId) : IRequest<UserDto?>;
