using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IUserRepository
{
    /// <summary>Returns the user with the given ID, or <see langword="null"/> if not found.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    /// <summary>Returns the user with the given username (case-insensitive), or <see langword="null"/> if not found.</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);
    /// <summary>Returns the user with the given email address, or <see langword="null"/> if not found.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    /// <summary>Persists a new user and populates its generated ID.</summary>
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    /// <summary>Returns all users ordered by display name.</summary>
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);
}
