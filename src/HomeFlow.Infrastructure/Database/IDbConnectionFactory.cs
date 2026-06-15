using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public interface IDbConnectionFactory
{
    NpgsqlConnection CreateConnection();
}
