using GameList.Application.Features.Social.DTOs;
using MediatR;

namespace GameList.Application.Features.Social.Queries;

/// <summary>Devuelve todos los juegos marcados como comprados por el usuario autenticado.</summary>
public sealed record GetMyPurchasesQuery(int UserId) : IRequest<IReadOnlyList<GameSummaryDto>>;
