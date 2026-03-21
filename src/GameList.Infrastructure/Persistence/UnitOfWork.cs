using GameList.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace GameList.Infrastructure.Persistence;

/// <summary>
/// Implementación de <see cref="IUnitOfWork"/> usando EF Core.
/// Gestiona transacciones explícitas sobre el <see cref="AppDbContext"/>.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext context;
    private IDbContextTransaction? currentTransaction;

    public UnitOfWork(AppDbContext context)
    {
        this.context = context;
    }

    /// <inheritdoc/>
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is null) return;
        await currentTransaction.CommitAsync(cancellationToken);
        await currentTransaction.DisposeAsync();
        currentTransaction = null;
    }

    /// <inheritdoc/>
    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is null) return;
        await currentTransaction.RollbackAsync(cancellationToken);
        await currentTransaction.DisposeAsync();
        currentTransaction = null;
    }
}
