using HomeFlow.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeFlow.Infrastructure.Tests.Repositories;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var runner = new MigrationRunner(new NpgsqlConnectionFactory(ConnectionString));
        await runner.RunAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    public async Task CleanTablesAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM household_tasks; DELETE FROM rotation_entries; DELETE FROM recurring_task_templates; DELETE FROM users;",
            conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
