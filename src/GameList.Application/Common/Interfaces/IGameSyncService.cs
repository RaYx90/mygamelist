namespace GameList.Application.Common.Interfaces;

public interface IGameSyncService
{
    Task SyncAsync(int year, CancellationToken cancellationToken = default);
}
