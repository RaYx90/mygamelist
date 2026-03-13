using GameList.Application.Features.Social.DTOs;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Consulta MediatR para obtener los favoritos y compras de cada miembro del grupo de un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario cuyo grupo se consulta.</param>
public sealed record GetGroupMembersGamesQuery(int UserId) : IRequest<IReadOnlyList<MemberGamesDto>>;
