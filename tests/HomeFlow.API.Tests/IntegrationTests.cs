using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.DTOs.Tasks;

namespace HomeFlow.API.Tests;

public class IntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public IntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<(string Token, Guid UserId)> RegisterAndGetTokenAsync(string username)
    {
        var request = new RegisterRequest(username, $"{username}@test.com", "Password123!", username);
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);
        var content = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = content.GetProperty("token").GetString()!;
        var userId = Guid.Parse(content.GetProperty("userId").GetString()!);
        return (token, userId);
    }

    private void SetAuth(string token)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("healthy");
    }

    [Fact]
    public async Task AuthFlow_RegisterLoginAndAccessProtectedEndpoint()
    {
        var registerRequest = new RegisterRequest("authtest", "authtest@test.com", "Password123!", "Auth Test");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerContent = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = registerContent.GetProperty("token").GetString()!;
        token.Should().NotBeNullOrEmpty();

        var loginRequest = new LoginRequest("authtest", "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        SetAuth(token);
        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meContent = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        meContent.GetProperty("username").GetString().Should().Be("authtest");

        _client.DefaultRequestHeaders.Authorization = null;
        var unauthResponse = await _client.GetAsync("/api/auth/me");
        unauthResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TaskFlow_CreateCompleteAndVerifyStatus()
    {
        var (token, _) = await RegisterAndGetTokenAsync("taskflow");
        SetAuth(token);

        var createRequest = new CreateTaskRequest("Integration test task", null, DateTime.UtcNow.AddDays(1), null);
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = created.GetProperty("id").GetString()!;

        var completeResponse = await _client.PatchAsync($"/api/tasks/{taskId}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completed = await completeResponse.Content.ReadFromJsonAsync<JsonElement>();
        completed.GetProperty("status").GetInt32().Should().Be(2);
        completed.GetProperty("completedAt").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecurringFlow_CreateTemplateGenerateAndVerifyRotation()
    {
        var (token1, userId1) = await RegisterAndGetTokenAsync("rot1");
        var (_, userId2) = await RegisterAndGetTokenAsync("rot2");
        SetAuth(token1);

        var createRequest = new CreateRecurringTaskRequest("Rotation test", null, 3, new List<Guid> { userId1, userId2 });
        var createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var templateId = created.GetProperty("id").GetString()!;

        var gen1 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        gen1.StatusCode.Should().Be(HttpStatusCode.Created);
        var task1 = await gen1.Content.ReadFromJsonAsync<JsonElement>();
        task1.GetProperty("assignedToUserId").GetString().Should().Be(userId1.ToString());
        task1.GetProperty("taskType").GetInt32().Should().Be(1);

        var gen2 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        var task2 = await gen2.Content.ReadFromJsonAsync<JsonElement>();
        task2.GetProperty("assignedToUserId").GetString().Should().Be(userId2.ToString());

        var gen3 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        var task3 = await gen3.Content.ReadFromJsonAsync<JsonElement>();
        task3.GetProperty("assignedToUserId").GetString().Should().Be(userId1.ToString());
    }

    [Fact]
    public async Task Users_List_RequiresAuthAndReturnsMembers()
    {
        var unauth = await _client.GetAsync("/api/users");
        unauth.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var (token, _) = await RegisterAndGetTokenAsync("userslist");
        SetAuth(token);

        var response = await _client.GetAsync("/api/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("userslist");
        body.Should().NotContain("passwordHash");
    }
}
