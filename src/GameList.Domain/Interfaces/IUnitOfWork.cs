namespace GameList.Domain.Interfaces;

/// <summary>
/// Abstracción para manejar transacciones de base de datos de forma explícita.
/// Permite agrupar múltiples operaciones en una única transacción atómica,
/// evitando estados intermedios visibles para los usuarios.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Inicia una transacción explícita.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirma la transacción actual, haciendo permanentes todos los cambios.
    /// </summary>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Revierte la transacción actual, descartando todos los cambios.
    /// </summary>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
