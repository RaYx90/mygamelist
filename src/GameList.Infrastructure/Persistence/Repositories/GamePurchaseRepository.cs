using GameList.Domain.Entities;
using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación EF Core del repositorio de juegos marcados como comprados.
/// </summary>
internal sealed class GamePurchaseRepository : IGamePurchaseRepository
{
    private readonly AppDbContext context;

    /// <summary>
    /// Inicializa el repositorio con el contexto de base de datos.
    /// </summary>
    /// <param name="context">Contexto de EF Core.</param>
    public GamePurchaseRepository(AppDbContext context) => this.context = context;

    /// <summary>
    /// Obtiene el registro de compra de un usuario para un juego concreto.
    /// </summary>
    /// <param name="userId">Identificador del usuario.</param>
    /// <param name="gameId">Identificador del juego.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>El registro de compra encontrado, o <c>null</c> si no existe.</returns>
    public Task<GamePurchaseEntity?> GetAsync(int userId, int gameId, CancellationToken ct) =>
        context.GamePurchases.FirstOrDefaultAsync(p => p.UserId == userId && p.GameId == gameId, ct);

    /// <summary>
    /// Devuelve los registros de compra de una lista de usuarios, incluyendo datos del juego.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de compras con datos del juego incluidos.</returns>
    public async Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAsync(IReadOnlyList<int> userIds, CancellationToken ct) =>
        await context.GamePurchases
            .Include(p => p.Game).ThenInclude(g => g!.Releases)
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync(ct);

    /// <summary>
    /// Devuelve los registros de compra de una lista de usuarios filtrados por un conjunto de juegos.
    /// </summary>
    /// <param name="userIds">Lista de identificadores de usuarios.</param>
    /// <param name="gameIds">Lista de identificadores de juegos.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Compras que coinciden con los usuarios y juegos indicados.</returns>
    public async Task<IReadOnlyList<GamePurchaseEntity>> GetByUserIdsAndGameIdsAsync(IReadOnlyList<int> userIds, IReadOnlyList<int> gameIds, CancellationToken ct) =>
        await context.GamePurchases
            .Where(p => userIds.Contains(p.UserId) && gameIds.Contains(p.GameId))
            .ToListAsync(ct);

    /// <summary>
    /// Añade un nuevo registro de compra al contexto.
    /// </summary>
    /// <param name="purchase">Entidad de compra a añadir.</param>
    /// <param name="ct">Token de cancelación.</param>
    public async Task AddAsync(GamePurchaseEntity purchase, CancellationToken ct) =>
        await context.GamePurchases.AddAsync(purchase, ct);

    /// <summary>
    /// Elimina un registro de compra del contexto.
    /// </summary>
    /// <param name="purchase">Entidad de compra a eliminar.</param>
    public void Remove(GamePurchaseEntity purchase) => context.GamePurchases.Remove(purchase);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    public Task SaveChangesAsync(CancellationToken ct) => context.SaveChangesAsync(ct);
}
