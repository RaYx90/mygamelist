using GameList.Application.Common.Interfaces;
using GameList.Application.Features.Sync.Commands;
using GameList.Domain.Interfaces;
using MediatR;

namespace GameList.Infrastructure.BackgroundServices;

/// <summary>
/// Implementación del servicio de sincronización de juegos que delega en MediatR el envío del comando <see cref="SyncGamesCommand"/>.
/// </summary>
internal sealed class GameSyncService : IGameSyncService
{
    private readonly IMediator mediator;

    /// <summary>
    /// Inicializa el servicio con el mediador de MediatR.
    /// </summary>
    /// <param name="mediator">Mediador para enviar comandos.</param>
    public GameSyncService(IMediator mediator) => this.mediator = mediator;

    /// <summary>
    /// Envía el comando de sincronización para el año indicado.
    /// </summary>
    /// <param name="year">Año a sincronizar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task SyncAsync(int year, CancellationToken cancellationToken = default)
    {
        await mediator.Send(new SyncGamesCommand(year), cancellationToken);
    }
}
