using System.Reflection;
using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class MigrationRunner(IDbConnectionFactory db)
{
    public async Task RunAsync()
    {
        await using var connection = db.CreateConnection();
        await connection.OpenAsync();

        await CreateMigrationHistoryTableAsync(connection);

        var migrations = GetMigrationFiles();
        foreach (var (name, sql) in migrations)
        {
            if (await HasBeenAppliedAsync(connection, name))
                continue;

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                await cmd.ExecuteNonQueryAsync();

                await using var recordCmd = new NpgsqlCommand(
                    "INSERT INTO migration_history (migration_name) VALUES (@name)", connection, transaction);
                recordCmd.Parameters.AddWithValue("name", name);
                await recordCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    private static async Task CreateMigrationHistoryTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS migration_history (
                id SERIAL PRIMARY KEY,
                migration_name VARCHAR(255) UNIQUE NOT NULL,
                applied_at TIMESTAMP NOT NULL DEFAULT NOW()
            )
            """;
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<bool> HasBeenAppliedAsync(NpgsqlConnection connection, string migrationName)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM migration_history WHERE migration_name = @name", connection);
        cmd.Parameters.AddWithValue("name", migrationName);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    private static List<(string Name, string Sql)> GetMigrationFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = "HomeFlow.Infrastructure.Database.Migrations.";

        return assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix) && n.EndsWith(".sql"))
            .OrderBy(n => n)
            .Select(resourceName =>
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
                var name = resourceName[prefix.Length..];
                return (name, sql);
            })
            .ToList();
    }
}
