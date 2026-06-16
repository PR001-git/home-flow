using System.Text.RegularExpressions;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.DTOs.Users;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Interfaces;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class UserService(IUserRepository userRepository, IJwtTokenProvider jwtTokenProvider)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3 || request.Username.Length > 50)
            throw new ValidationException("Invalid username: must be between 3 and 50 characters.");

        if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ValidationException("Invalid email format.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Invalid password: must be at least 8 characters.");

        var existingByUsername = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (existingByUsername is not null)
            throw new ValidationException("A user with this username already exists.");

        var existingByEmail = await userRepository.GetByEmailAsync(request.Email, ct);
        if (existingByEmail is not null)
            throw new ValidationException("A user with this email already exists.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName,
            CreatedAt = DateTime.UtcNow
        };

        var created = await userRepository.CreateAsync(user, ct);
        var token = jwtTokenProvider.GenerateToken(created);

        return new AuthResponse(created.Id, created.Username, created.DisplayName, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.Username, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ValidationException("Invalid credentials.");

        var token = jwtTokenProvider.GenerateToken(user);
        return new AuthResponse(user.Id, user.Username, user.DisplayName, token);
    }

    public async Task<UserResponse> GetByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct);
        if (user is null)
            throw new NotFoundException($"User with ID {userId} not found.");

        return new UserResponse(user.Id, user.Username, user.Email, user.DisplayName, user.CreatedAt);
    }

    public async Task<IEnumerable<UserSummaryResponse>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var users = await userRepository.GetAllAsync(ct);
        return users.Select(u => new UserSummaryResponse(u.Id, u.Username, u.DisplayName)).ToList();
    }
}
