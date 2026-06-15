using FluentAssertions;
using HomeFlow.Domain.Entities;
using HomeFlow.Infrastructure.Repositories;

namespace HomeFlow.Infrastructure.Tests.Repositories;

[Collection("Database")]
public class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private readonly UserRepository _sut;

    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _sut = new UserRepository(new HomeFlow.Infrastructure.Database.NpgsqlConnectionFactory(fixture.ConnectionString));
    }

    public Task InitializeAsync() => _fixture.CleanTablesAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task CreateAsync_InsertsAndReturnsUserWithId()
    {
        var user = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            DisplayName = "Test",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _sut.CreateAsync(user);

        result.Id.Should().NotBe(Guid.Empty);
        result.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsUser()
    {
        var user = await _sut.CreateAsync(new User
        {
            Username = "findme",
            Email = "find@example.com",
            PasswordHash = "hash",
            DisplayName = "Find Me",
            CreatedAt = DateTime.UtcNow
        });

        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Username.Should().Be("findme");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUsernameAsync_ExistingUser_ReturnsUser()
    {
        await _sut.CreateAsync(new User
        {
            Username = "byname",
            Email = "byname@example.com",
            PasswordHash = "hash",
            DisplayName = "By Name",
            CreatedAt = DateTime.UtcNow
        });

        var result = await _sut.GetByUsernameAsync("byname");

        result.Should().NotBeNull();
        result!.Username.Should().Be("byname");
    }

    [Fact]
    public async Task GetByEmailAsync_ExistingUser_ReturnsUser()
    {
        await _sut.CreateAsync(new User
        {
            Username = "byemail",
            Email = "byemail@example.com",
            PasswordHash = "hash",
            DisplayName = "By Email",
            CreatedAt = DateTime.UtcNow
        });

        var result = await _sut.GetByEmailAsync("byemail@example.com");

        result.Should().NotBeNull();
        result!.Email.Should().Be("byemail@example.com");
    }
}
