using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public interface IDbConnectionFactory
{
    /// <summary>Creates and returns a new, unopened <see cref="NpgsqlConnection"/>.</summary>
    NpgsqlConnection CreateConnection();
}
