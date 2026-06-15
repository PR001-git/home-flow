using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class UserRepository(IDbConnectionFactory db) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await ReadUserAsync(cmd);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE LOWER(username) = LOWER(@username)", conn);
        cmd.Parameters.AddWithValue("username", username);
        return await ReadUserAsync(cmd);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("email", email);
        return await ReadUserAsync(cmd);
    }

    public async Task<User> CreateAsync(User user)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO users (username, email, password_hash, display_name, created_at)
            VALUES (@username, @email, @passwordHash, @displayName, @createdAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("username", user.Username);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("displayName", user.DisplayName);
        cmd.Parameters.AddWithValue("createdAt", user.CreatedAt);

        user.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return user;
    }

    private static async Task<User?> ReadUserAsync(NpgsqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new User
        {
            Id = reader.GetGuid(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            DisplayName = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5)
        };
    }
}
