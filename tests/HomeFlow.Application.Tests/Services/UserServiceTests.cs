using FluentAssertions;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Interfaces;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenProvider _jwtTokenProvider;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _jwtTokenProvider = Substitute.For<IJwtTokenProvider>();
        _sut = new UserService(_userRepository, _jwtTokenProvider);
    }

    [Fact]
    public async Task Register_ValidInput_CreatesUserAndReturnsAuthResponse()
    {
        var request = new RegisterRequest("pedro", "pedro@example.com", "Password123!", "Pedro");
        _userRepository.GetByUsernameAsync("pedro").Returns((User?)null);
        _userRepository.GetByEmailAsync("pedro@example.com").Returns((User?)null);
        _userRepository.CreateAsync(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            user.Id = Guid.NewGuid();
            return user;
        });
        _jwtTokenProvider.GenerateToken(Arg.Any<User>()).Returns("jwt-token");

        var result = await _sut.RegisterAsync(request);

        result.Username.Should().Be("pedro");
        result.DisplayName.Should().Be("Pedro");
        result.Token.Should().Be("jwt-token");
        await _userRepository.Received(1).CreateAsync(Arg.Is<User>(u =>
            u.Username == "pedro" && u.Email == "pedro@example.com" && u.DisplayName == "Pedro"));
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsAuthResponse()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "pedro",
            PasswordHash = passwordHash,
            DisplayName = "Pedro"
        };
        _userRepository.GetByUsernameAsync("pedro").Returns(user);
        _jwtTokenProvider.GenerateToken(user).Returns("jwt-token");

        var result = await _sut.LoginAsync(new LoginRequest("pedro", "Password123!"));

        result.Username.Should().Be("pedro");
        result.Token.Should().Be("jwt-token");
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsValidationException()
    {
        var user = new User
        {
            Username = "pedro",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        };
        _userRepository.GetByUsernameAsync("pedro").Returns(user);

        var act = () => _sut.LoginAsync(new LoginRequest("pedro", "WrongPassword!"));

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*credentials*");
    }

    [Fact]
    public async Task GetAllUsers_ReturnsSummariesWithoutSecrets()
    {
        _userRepository.GetAllAsync().Returns(new[]
        {
            new User { Id = Guid.NewGuid(), Username = "pedro", Email = "p@test.com", PasswordHash = "secret", DisplayName = "Pedro" }
        });

        var result = (await _sut.GetAllUsersAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Username.Should().Be("pedro");
        result[0].DisplayName.Should().Be("Pedro");
    }
}
