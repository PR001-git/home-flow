using FluentAssertions;
using HomeFlow.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeFlow.Infrastructure.Tests.Database;

public class MigrationRunnerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [Fact]
    public async Task RunAsync_CreatesMigrationHistoryTable()
    {
        var runner = new MigrationRunner(new NpgsqlConnectionFactory(_postgres.GetConnectionString()));

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'migration_history'", conn);
        var result = await cmd.ExecuteScalarAsync();
        ((long)result!).Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_RunsTwice_IsIdempotent()
    {
        var runner = new MigrationRunner(new NpgsqlConnectionFactory(_postgres.GetConnectionString()));

        await runner.RunAsync();
        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM migration_history", conn);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        count.Should().Be(5);
    }

    [Fact]
    public async Task RunAsync_CreatesAllTables()
    {
        var runner = new MigrationRunner(new NpgsqlConnectionFactory(_postgres.GetConnectionString()));

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        var tables = new[] { "users", "recurring_task_templates", "household_tasks", "rotation_entries" };
        foreach (var table in tables)
        {
            await using var cmd = new NpgsqlCommand(
                $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{table}'", conn);
            var result = (long)(await cmd.ExecuteScalarAsync())!;
            result.Should().Be(1, $"table '{table}' should exist");
        }
    }

    [Fact]
    public async Task RunAsync_SeedsData()
    {
        var runner = new MigrationRunner(new NpgsqlConnectionFactory(_postgres.GetConnectionString()));

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users", conn);
        var userCount = (long)(await cmd.ExecuteScalarAsync())!;
        userCount.Should().Be(4);
    }
}
