using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class NpgsqlConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public NpgsqlConnection CreateConnection() => new NpgsqlConnection(connectionString);
}
