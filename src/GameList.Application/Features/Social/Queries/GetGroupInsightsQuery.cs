using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Consulta MediatR para obtener el resumen de favoritos y compras del grupo de un usuario.
/// </summary>
/// <param name="UserId">Identificador del usuario cuyo grupo se consulta.</param>
public sealed record GetGroupInsightsQuery(int UserId) : IRequest<IReadOnlyList<GroupInsightDto>>;
