using GameList.Application.Common.Mappers;
using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

/// <summary>
/// Handler MediatR que busca lanzamientos por nombre en un año completo,
/// los agrupa por día y fusiona ediciones del mismo juego en distintas plataformas.
/// </summary>
public sealed class SearchReleasesHandler : IRequestHandler<SearchReleasesQuery, IReadOnlyList<CalendarDayDto>>
{
    private readonly IGameReleaseRepository releaseRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de lanzamientos.
    /// </summary>
    /// <param name="releaseRepository">Repositorio de lanzamientos.</param>
    public SearchReleasesHandler(IGameReleaseRepository releaseRepository)
        => this.releaseRepository = releaseRepository;

    /// <summary>
    /// Busca lanzamientos que coincidan con el nombre en el año indicado,
    /// fusiona entradas del mismo juego y agrupa el resultado por día.
    /// </summary>
    /// <param name="request">Consulta con año y nombre parcial.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de días del calendario con sus lanzamientos coincidentes.</returns>
    public async Task<IReadOnlyList<CalendarDayDto>> Handle(SearchReleasesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Length < 2)
            return [];

        var releases = await releaseRepository.SearchByNameAsync(request.Year, request.Name, cancellationToken);

        return releases
            .GroupBy(r => r.ReleaseDate)
            .OrderBy(g => g.Key)
            .Select(g => new CalendarDayDto(
                Date: g.Key,
                Releases: g
                    .GroupBy(r => r.GameId)
                    .Select(gameGroup =>
                    {
                        var dtos = gameGroup.Select(GameMapper.ToDto).ToList();
                        if (dtos.Count == 1) return dtos[0];
                        var allLabels = dtos
                            .Select(d => d.PlatformAbbreviation ?? d.PlatformName)
                            .Distinct()
                            .ToList()
                            .AsReadOnly();
                        return dtos[0] with { AllPlatformLabels = allLabels };
                    })
                    .ToList()
                    .AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }
}
