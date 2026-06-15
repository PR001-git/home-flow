using HomeFlow.Domain.Entities;

namespace HomeFlow.Application.Interfaces;

public interface IJwtTokenProvider
{
    string GenerateToken(User user);
}
