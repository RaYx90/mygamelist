using GameList.Application.Features.Platforms.DTOs;
using MediatR;

namespace GameList.Application.Features.Platforms.Queries;

/// <summary>
/// Consulta MediatR para obtener todas las plataformas disponibles, ordenadas por nombre.
/// </summary>
public sealed record GetPlatformsQuery : IRequest<IReadOnlyList<PlatformDto>>;
