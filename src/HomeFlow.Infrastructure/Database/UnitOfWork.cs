using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class UnitOfWork(IDbConnectionFactory db) : IUnitOfWork, IAsyncDisposable
{
    private NpgsqlConnection? _connection;

    public NpgsqlTransaction? Transaction { get; private set; }

    public async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_connection is null)
        {
            _connection = db.CreateConnection();
            await _connection.OpenAsync();
        }
        return _connection;
    }

    public async Task BeginTransactionAsync()
    {
        var connection = await GetConnectionAsync();
        Transaction = await connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (Transaction is null) return;
        await Transaction.CommitAsync();
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    public async Task RollbackAsync()
    {
        if (Transaction is null) return;
        await Transaction.RollbackAsync();
        await Transaction.DisposeAsync();
        Transaction = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (Transaction is not null)
            await Transaction.DisposeAsync();
        if (_connection is not null)
            await _connection.DisposeAsync();
    }
}
