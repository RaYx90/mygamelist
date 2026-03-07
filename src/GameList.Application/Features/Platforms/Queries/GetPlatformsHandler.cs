using GameList.Application.Common.Mappers;
using GameList.Application.Features.Platforms.DTOs;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Application.Features.Platforms.Queries;

public sealed class GetPlatformsHandler
    : IRequestHandler<GetPlatformsQuery, IReadOnlyList<PlatformDto>>
{
    private readonly IPlatformRepository _platformRepository;

    public GetPlatformsHandler(IPlatformRepository platformRepository)
    {
        _platformRepository = platformRepository;
    }

    public async Task<IReadOnlyList<PlatformDto>> Handle(
        GetPlatformsQuery request,
        CancellationToken cancellationToken)
    {
        var platforms = await _platformRepository.GetAllAsync(cancellationToken);

        return platforms
            .OrderBy(p => p.Name)
            .Select(GameMapper.ToPlatformDto)
            .ToList()
            .AsReadOnly();
    }
}
