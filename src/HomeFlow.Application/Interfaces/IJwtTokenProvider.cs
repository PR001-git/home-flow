using HomeFlow.Domain.Entities;

namespace HomeFlow.Application.Interfaces;

public interface IJwtTokenProvider
{
    /// <summary>Generates a signed JWT for the given user.</summary>
    string GenerateToken(User user);
}
