namespace HomeFlow.Application.DTOs.Users;

public record UserResponse(Guid Id, string Username, string Email, string DisplayName, DateTime CreatedAt);
