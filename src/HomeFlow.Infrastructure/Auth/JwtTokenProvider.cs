using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeFlow.Application.Interfaces;
using HomeFlow.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HomeFlow.Infrastructure.Auth;

public class JwtTokenProvider(string key, string issuer, string audience, int expirationMinutes) : IJwtTokenProvider
{
    /// <summary>Generates a signed HS256 JWT containing the user's ID, username, and email claims.</summary>
    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
