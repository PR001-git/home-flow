using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using HomeFlow.Domain.Entities;
using HomeFlow.Infrastructure.Auth;

namespace HomeFlow.Infrastructure.Tests.Auth;

public class JwtTokenProviderTests
{
    private readonly JwtTokenProvider _sut;

    public JwtTokenProviderTests()
    {
        _sut = new JwtTokenProvider(
            key: "this-is-a-very-long-secret-key-for-testing-purposes-at-least-32-bytes",
            issuer: "HomeFlow",
            audience: "HomeFlow",
            expirationMinutes: 60);
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "pedro",
            Email = "pedro@example.com",
            DisplayName = "Pedro"
        };

        var token = _sut.GenerateToken(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "sub" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == "pedro");
    }

    [Fact]
    public void GenerateToken_SetsExpiration()
    {
        var user = new User { Id = Guid.NewGuid(), Username = "test", Email = "t@t.com", DisplayName = "T" };

        var token = _sut.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwt.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(61));
    }
}
