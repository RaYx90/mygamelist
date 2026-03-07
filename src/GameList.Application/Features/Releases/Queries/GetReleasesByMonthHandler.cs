using GameList.Application.Common.Mappers;
using GameList.Application.Features.Releases.DTOs;
using GameList.Domain.Ports;
using GameList.Domain.ValueObjects;
using MediatR;

namespace GameList.Application.Features.Releases.Queries;

public sealed class GetReleasesByMonthHandler
    : IRequestHandler<GetReleasesByMonthQuery, IReadOnlyList<CalendarDayDto>>
{
    private readonly IGameReleaseRepository _releaseRepository;

    public GetReleasesByMonthHandler(IGameReleaseRepository releaseRepository)
    {
        _releaseRepository = releaseRepository;
    }

    public async Task<IReadOnlyList<CalendarDayDto>> Handle(
        GetReleasesByMonthQuery request,
        CancellationToken cancellationToken)
    {
        var dateRange = DateRangeValue.ForMonth(request.Year, request.Month);

        var releases = await _releaseRepository.GetByDateRangeAsync(
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

    private static string GetBaseName(string name)
    {
        var sep = name.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
        return (sep > 0 ? name[..sep] : name).Trim().ToUpperInvariant();
    }
}
