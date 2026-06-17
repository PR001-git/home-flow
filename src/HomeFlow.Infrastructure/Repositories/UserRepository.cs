using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Database;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class UserRepository(IDbConnectionFactory db) : IUserRepository
{
    /// <summary>Returns the user with the given ID, or <see langword="null"/> if not found.</summary>
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await ReadUserAsync(cmd, ct);
    }

    /// <summary>Returns the user with the given username (case-insensitive), or <see langword="null"/> if not found.</summary>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE LOWER(username) = LOWER(@username)", conn);
        cmd.Parameters.AddWithValue("username", username);
        return await ReadUserAsync(cmd, ct);
    }

    /// <summary>Returns the user with the given email address, or <see langword="null"/> if not found.</summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("email", email);
        return await ReadUserAsync(cmd, ct);
    }

    /// <summary>Inserts a new user row and populates the entity's generated ID.</summary>
    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync(ct);
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

        user.Id = (Guid)(await cmd.ExecuteScalarAsync(ct))!;
        return user;
    }

    /// <summary>Returns all users ordered by display name.</summary>
    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = db.CreateConnection();
        await conn.OpenAsync(ct);
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users ORDER BY display_name", conn);

        var users = new List<User>();
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            users.Add(new User
            {
                Id = reader.Get<Guid>("id"),
                Username = reader.Get<string>("username"),
                Email = reader.Get<string>("email"),
                PasswordHash = reader.Get<string>("password_hash"),
                DisplayName = reader.Get<string>("display_name"),
                CreatedAt = reader.Get<DateTime>("created_at")
            });
        }
        return users;
    }

    private static async Task<User?> ReadUserAsync(NpgsqlCommand cmd, CancellationToken ct)
    {
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct)) return null;

        return new User
        {
            Id = reader.Get<Guid>("id"),
            Username = reader.Get<string>("username"),
            Email = reader.Get<string>("email"),
            PasswordHash = reader.Get<string>("password_hash"),
            DisplayName = reader.Get<string>("display_name"),
            CreatedAt = reader.Get<DateTime>("created_at")
        };
    }
}
