using GameList.Application.Features.Platforms.DTOs;
using MediatR;

namespace GameList.Application.Features.Platforms.Queries;

public sealed record GetPlatformsQuery : IRequest<IReadOnlyList<PlatformDto>>;
