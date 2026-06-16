using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class UnitOfWork(IDbConnectionFactory db) : IUnitOfWork, IAsyncDisposable
{
    private NpgsqlConnection? _connection;

    public NpgsqlTransaction? Transaction { get; private set; }

    /// <summary>Returns the shared connection, opening it on first call.</summary>
    public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct = default)
    {
        if (_connection is null)
        {
            _connection = db.CreateConnection();
            await _connection.OpenAsync(ct);
        }
        return _connection;
    }

    /// <summary>Opens a connection (if not already open) and starts a new database transaction.</summary>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        var connection = await GetConnectionAsync(ct);
        Transaction = await connection.BeginTransactionAsync(ct);
    }

    /// <summary>Commits the active transaction and disposes it.</summary>
    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (Transaction is null) return;
        await Transaction.CommitAsync(ct);
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    /// <summary>Rolls back the active transaction and disposes it.</summary>
    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (Transaction is null) return;
        await Transaction.RollbackAsync(ct);
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    /// <summary>Disposes the active transaction and connection.</summary>
    public async ValueTask DisposeAsync()
    {
        if (Transaction is not null)
            await Transaction.DisposeAsync();
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
