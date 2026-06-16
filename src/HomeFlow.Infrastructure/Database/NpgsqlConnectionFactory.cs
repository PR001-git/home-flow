using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    /// <summary>Creates and returns a new, unopened <see cref="NpgsqlConnection"/> using the configured connection string.</summary>
    public NpgsqlConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
