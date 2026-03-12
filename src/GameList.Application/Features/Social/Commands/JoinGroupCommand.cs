using GameList.Application.Features.Social.DTOs;
using MediatR;

namespace GameList.Application.Features.Social.Commands;

// Devuelve GroupDto? en lugar de bool para que el frontend pueda actualizar el groupId en el store.
// null indica código inválido → el endpoint responde 400 Bad Request.
public sealed record JoinGroupCommand(int UserId, string InviteCode) : IRequest<GroupDto?>;
