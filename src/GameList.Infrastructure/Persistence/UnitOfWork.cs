using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameList.Infrastructure.Persistence;

/// <summary>
/// Implementación de <see cref="IUnitOfWork"/> usando EF Core.
/// Usa <see cref="IExecutionStrategy"/> para compatibilidad con estrategias de retry
/// (ej: NpgsqlRetryingExecutionStrategy), que no permiten BeginTransaction directo.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext context;

    public UnitOfWork(AppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async ct =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);
            await operation(ct);
            await transaction.CommitAsync(ct);
        }, cancellationToken);
    }
}
