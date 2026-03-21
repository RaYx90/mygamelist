namespace GameList.Domain.Interfaces;

/// <summary>
/// Abstracción para ejecutar operaciones dentro de una transacción atómica.
/// Compatible con estrategias de retry (ej: NpgsqlRetryingExecutionStrategy).
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Ejecuta la operación dada dentro de una transacción atómica.
    /// Si la estrategia de ejecución tiene retry habilitado, toda la operación
    /// (incluida la transacción) se reintenta como una unidad.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
