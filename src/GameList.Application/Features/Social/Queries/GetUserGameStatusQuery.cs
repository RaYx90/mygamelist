using GameList.Application.Features.Social.DTOs;
using MediatR;
namespace GameList.Application.Features.Social.Queries;

/// <summary>
/// Consulta MediatR para obtener el estado (favoritos y compras) de un usuario para un conjunto de juegos,
/// incluyendo el estado de los miembros de su grupo.
/// </summary>
/// <param name="UserId">Identificador del usuario.</param>
/// <param name="GameIds">Lista de identificadores de juegos a consultar.</param>
public sealed record GetUserGameStatusQuery(int UserId, IReadOnlyList<int> GameIds) : IRequest<UserGameStatusDto>;
