using MediatR;

namespace GameList.Application.Features.Sync.Commands;

public sealed record SyncGamesCommand(int Year) : IRequest<SyncResultDto>;

public sealed record SyncResultDto(
    bool Success,
    int GamesProcessed,
    int PlatformsProcessed,
    string? ErrorMessage = null
);
