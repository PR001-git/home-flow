namespace HomeFlow.Domain.Repositories;

public interface IUnitOfWork
{
    /// <summary>Opens a connection (if not already open) and starts a new database transaction.</summary>
    Task BeginTransactionAsync(CancellationToken ct = default);
    /// <summary>Commits the active transaction and disposes it.</summary>
    Task CommitAsync(CancellationToken ct = default);
    /// <summary>Rolls back the active transaction and disposes it.</summary>
    Task RollbackAsync(CancellationToken ct = default);
}
