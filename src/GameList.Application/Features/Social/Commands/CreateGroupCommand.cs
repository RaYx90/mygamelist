using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Commands;

/// <summary>
/// Comando MediatR para crear un nuevo grupo social y asignar al usuario como primer miembro.
/// </summary>
/// <param name="UserId">Identificador del usuario que crea el grupo.</param>
/// <param name="GroupName">Nombre del grupo a crear.</param>
public sealed record CreateGroupCommand(int UserId, string GroupName) : IRequest<GroupDto>;
