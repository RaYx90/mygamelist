using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Sync.Commands;
using GameList.Domain.Ports;
using MediatR;

namespace GameList.Infrastructure.BackgroundServices;

internal sealed class GameSyncService : IGameSyncService
{
    private readonly IMediator _mediator;

    public GameSyncService(IMediator mediator) => _mediator = mediator;

    public async Task SyncAsync(int year, CancellationToken cancellationToken = default)
    {
        await _mediator.Send(new SyncGamesCommand(year), cancellationToken);
    }
}
