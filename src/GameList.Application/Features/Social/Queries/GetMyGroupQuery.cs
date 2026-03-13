using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Consulta MediatR para obtener los datos del grupo al que pertenece un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
public sealed record GetMyGroupQuery(int UserId) : IRequest<GroupDto?>;
