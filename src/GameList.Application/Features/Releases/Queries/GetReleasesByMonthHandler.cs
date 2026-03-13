using GameList.Application.Common.Mappers;
using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Interfaces;
using GameList.Domain.ValueObjects;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

/// <summary>
/// Handler MediatR que obtiene los lanzamientos de un mes, los agrupa por día y fusiona ediciones del mismo juego.
/// </summary>
public sealed class GetReleasesByMonthHandler
    : IRequestHandler<GetReleasesByMonthQuery, IReadOnlyList<CalendarDayDto>>
{
    private readonly IGameReleaseRepository releaseRepository;

    /// <summary>
    /// Inicializa el handler con el repositorio de lanzamientos.
    /// </summary>
    /// <param name="releaseRepository">Repositorio de lanzamientos.</param>
    public GetReleasesByMonthHandler(IGameReleaseRepository releaseRepository)
    {
        this.releaseRepository = releaseRepository;
    }

    /// <summary>
    /// Obtiene los lanzamientos del mes, fusiona entradas del mismo juego en distintas plataformas
    /// y agrupa el resultado por día de calendario.
    /// </summary>
    /// <param name="request">Consulta con año, mes y filtros opcionales.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Lista de días del calendario con sus lanzamientos.</returns>
    public async Task<IReadOnlyList<CalendarDayDto>> Handle(
        GetReleasesByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = DateRangeValue.ForMonth(request.Year, request.Month);

        var releases = await releaseRepository.GetByDateRangeAsync(
            dateRange,
            request.PlatformId,
            request.Category,
            request.IsIndie,
            cancellationToken);

        return releases
            .GroupBy(r => r.ReleaseDate)
            .OrderBy(g => g.Key)
            .Select(g => new CalendarDayDto(
                Date: g.Key,
                Releases: g
                    // Step 1: merge same game across platforms (same GameId, same day)
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
                    // Step 2: merge editions ("Game X" and "Game X: Deluxe Edition" → same entry)
                    .GroupBy(r => GetBaseName(r.GameName))
                    .Select(nameGroup =>
                    {
                        var entries = nameGroup
                            .OrderBy(r => r.GameName.Length) // prefer shortest (base) name
                            .ToList();
                        var allLabels = entries
                            .SelectMany(e => e.AllPlatformLabels)
                            .Distinct()
                            .ToList()
                            .AsReadOnly();
                        return entries[0] with { AllPlatformLabels = allLabels };
                    })
                    .ToList()
                    .AsReadOnly()))
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Extrae el nombre base de un juego eliminando el subtítulo tras los dos puntos.
    /// </summary>
    /// <param name="name">Nombre completo del juego.</param>
    /// <returns>Nombre base en mayúsculas para agrupar ediciones.</returns>
    private static string GetBaseName(string name)
    {
        var sep = name.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
        return (sep > 0 ? name[..sep] : name).Trim().ToUpperInvariant();
    }
}
