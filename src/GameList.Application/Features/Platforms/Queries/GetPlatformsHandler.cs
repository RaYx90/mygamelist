using GameList.Application.Common.Mappers;
using GameList.Application.Features.Platforms.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Platforms.Queries;

/// <summary>
/// Handler MediatR que devuelve todas las plataformas ordenadas alfabéticamente.
/// </summary>
public sealed class GetPlatformsHandler
    : IRequestHandler<GetPlatformsQuery, IReadOnlyList<PlatformDto>>
{
    private readonly IPlatformRepository platformRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de plataformas.
    /// </summary>
    /// <param name="platformRepository">Repositorio de plataformas.</param>
    public GetPlatformsHandler(IPlatformRepository platformRepository)
    {
        this.platformRepository = platformRepository;
    }

    /// <summary>
    /// Obtiene todas las plataformas y las devuelve ordenadas por nombre.
    /// </summary>
    /// <param name="request">Consulta sin parámetros.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de DTOs de plataformas ordenados por nombre.</returns>
    public async Task<IReadOnlyList<PlatformDto>> Handle(
        GetPlatformsQuery request,
        CancellationToken cancellationToken)
    {
        var platforms = await platformRepository.GetAllAsync(cancellationToken);

        return platforms
            .OrderBy(p => p.Name)
            .Select(GameMapper.ToPlatformDto)
            .ToList()
            .AsReadOnly();
    }
}
