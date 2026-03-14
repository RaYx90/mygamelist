using GameList.Application.Features.Social.DTOs;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>Devuelve todos los favoritos del usuario autenticado.</summary>
public sealed record GetMyFavoritesQuery(int UserId) : IRequest<IReadOnlyList<GameSummaryDto>>;
