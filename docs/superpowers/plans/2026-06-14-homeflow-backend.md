# HomeFlow Backend Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a .NET C# / ASP.NET Web API for household task management with recurring chore rotation, one-off tasks, and JWT authentication — using Clean Architecture with raw Npgsql (no EF/Dapper/Mediator).

**Architecture:** Clean Architecture with four layers: Domain (entities, enums, interfaces — no dependencies), Application (services, DTOs, validation — depends on Domain), Infrastructure (Npgsql repositories, JWT, migrations — implements Domain interfaces), API (controllers, middleware, DI — wires everything). All data access uses raw SQL via Npgsql.

**Tech Stack:** .NET 8, ASP.NET Web API, PostgreSQL 16, Npgsql, BCrypt.Net-Next, xUnit, NSubstitute, FluentAssertions, Testcontainers

**Spec:** `docs/superpowers/specs/2026-06-14-home-flow-design.md`

**Scope:** This plan covers the backend only. Frontend (React + TypeScript) and DevOps (Docker, CI) are separate plans.

---

## File Structure

```
HomeFlow.sln
src/
├── HomeFlow.Domain/
│   ├── HomeFlow.Domain.csproj
│   ├── Enums/
│   │   ├── HouseholdTaskType.cs          # OneOff = 0, Recurring = 1
│   │   └── HouseholdTaskStatus.cs        # Pending = 0, InProgress = 1, Completed = 2, Overdue = 3
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── HouseholdTask.cs
│   │   ├── RecurringTaskTemplate.cs
│   │   └── RotationEntry.cs
│   └── Repositories/
│       ├── IUserRepository.cs
│       ├── ITaskRepository.cs
│       ├── IRecurringTaskTemplateRepository.cs
│       └── IRotationEntryRepository.cs
├── HomeFlow.Application/
│   ├── HomeFlow.Application.csproj
│   ├── DTOs/
│   │   ├── Auth/
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   └── AuthResponse.cs
│   │   ├── Tasks/
│   │   │   ├── CreateTaskRequest.cs
│   │   │   ├── UpdateTaskRequest.cs
│   │   │   ├── TaskResponse.cs
│   │   │   └── TaskFilterDto.cs
│   │   ├── RecurringTasks/
│   │   │   ├── CreateRecurringTaskRequest.cs
│   │   │   ├── UpdateRecurringTaskRequest.cs
│   │   │   └── RecurringTaskResponse.cs
│   │   └── Users/
│   │       └── UserResponse.cs
│   ├── Interfaces/
│   │   └── IJwtTokenProvider.cs
│   ├── Exceptions/
│   │   ├── ValidationException.cs
│   │   └── NotFoundException.cs
│   └── Services/
│       ├── UserService.cs
│       ├── TaskService.cs
│       └── RecurringTaskService.cs
├── HomeFlow.Infrastructure/
│   ├── HomeFlow.Infrastructure.csproj
│   ├── Database/
│   │   ├── MigrationRunner.cs
│   │   └── Migrations/
│   │       ├── 001_CreateUsersTable.sql
│   │       ├── 002_CreateRecurringTaskTemplatesTable.sql
│   │       ├── 003_CreateHouseholdTasksTable.sql
│   │       ├── 004_CreateRotationEntriesTable.sql
│   │       └── 005_SeedData.sql
│   ├── Repositories/
│   │   ├── UserRepository.cs
│   │   ├── TaskRepository.cs
│   │   ├── RecurringTaskTemplateRepository.cs
│   │   └── RotationEntryRepository.cs
│   └── Auth/
│       └── JwtTokenProvider.cs
└── HomeFlow.API/
    ├── HomeFlow.API.csproj
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── Middleware/
    │   └── ExceptionHandlingMiddleware.cs
    └── Controllers/
        ├── AuthController.cs
        ├── TasksController.cs
        ├── RecurringTasksController.cs
        └── HealthController.cs

tests/
├── HomeFlow.Application.Tests/
│   ├── HomeFlow.Application.Tests.csproj
│   └── Services/
│       ├── UserServiceTests.cs
│       ├── TaskServiceTests.cs
│       └── RecurringTaskServiceTests.cs
├── HomeFlow.Infrastructure.Tests/
│   ├── HomeFlow.Infrastructure.Tests.csproj
│   ├── Database/
│   │   └── MigrationRunnerTests.cs
│   ├── Repositories/
│   │   ├── DatabaseFixture.cs
│   │   └── UserRepositoryTests.cs
│   └── Auth/
│       └── JwtTokenProviderTests.cs
└── HomeFlow.API.Tests/
    ├── HomeFlow.API.Tests.csproj
    ├── CustomWebApplicationFactory.cs
    └── IntegrationTests.cs
```

---

## Task 1: Solution & Project Scaffolding

**Files:**
- Create: `HomeFlow.sln`
- Create: `src/HomeFlow.Domain/HomeFlow.Domain.csproj`
- Create: `src/HomeFlow.Application/HomeFlow.Application.csproj`
- Create: `src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj`
- Create: `src/HomeFlow.API/HomeFlow.API.csproj`
- Create: `tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj`
- Create: `tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj`
- Create: `tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj`

- [ ] **Step 1: Create solution and source projects**

```bash
dotnet new sln -n HomeFlow
dotnet new classlib -o src/HomeFlow.Domain
dotnet new classlib -o src/HomeFlow.Application
dotnet new classlib -o src/HomeFlow.Infrastructure
dotnet new webapi -o src/HomeFlow.API --no-openapi
```

- [ ] **Step 2: Create test projects**

```bash
dotnet new xunit -o tests/HomeFlow.Application.Tests
dotnet new xunit -o tests/HomeFlow.Infrastructure.Tests
dotnet new xunit -o tests/HomeFlow.API.Tests
```

- [ ] **Step 3: Add all projects to solution**

```bash
dotnet sln add src/HomeFlow.Domain/HomeFlow.Domain.csproj
dotnet sln add src/HomeFlow.Application/HomeFlow.Application.csproj
dotnet sln add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj
dotnet sln add src/HomeFlow.API/HomeFlow.API.csproj
dotnet sln add tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj
dotnet sln add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj
dotnet sln add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj
```

- [ ] **Step 4: Add project references (Clean Architecture dependencies)**

```bash
# Application depends on Domain
dotnet add src/HomeFlow.Application/HomeFlow.Application.csproj reference src/HomeFlow.Domain/HomeFlow.Domain.csproj

# Infrastructure depends on Domain and Application
dotnet add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj reference src/HomeFlow.Domain/HomeFlow.Domain.csproj
dotnet add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj reference src/HomeFlow.Application/HomeFlow.Application.csproj

# API depends on Application and Infrastructure (for DI wiring)
dotnet add src/HomeFlow.API/HomeFlow.API.csproj reference src/HomeFlow.Application/HomeFlow.Application.csproj
dotnet add src/HomeFlow.API/HomeFlow.API.csproj reference src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj

# Test projects reference their subjects
dotnet add tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj reference src/HomeFlow.Application/HomeFlow.Application.csproj
dotnet add tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj reference src/HomeFlow.Domain/HomeFlow.Domain.csproj
dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj reference src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj
dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj reference src/HomeFlow.Domain/HomeFlow.Domain.csproj
dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj reference src/HomeFlow.Application/HomeFlow.Application.csproj
dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj reference src/HomeFlow.API/HomeFlow.API.csproj
dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj reference src/HomeFlow.Application/HomeFlow.Application.csproj
dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj reference src/HomeFlow.Domain/HomeFlow.Domain.csproj
```

- [ ] **Step 5: Add NuGet packages**

```bash
# Application layer
dotnet add src/HomeFlow.Application/HomeFlow.Application.csproj package BCrypt.Net-Next

# Infrastructure layer
dotnet add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj package Npgsql
dotnet add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj package System.IdentityModel.Tokens.Jwt

# Test projects
dotnet add tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj package NSubstitute
dotnet add tests/HomeFlow.Application.Tests/HomeFlow.Application.Tests.csproj package FluentAssertions

dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj package NSubstitute
dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj package FluentAssertions
dotnet add tests/HomeFlow.Infrastructure.Tests/HomeFlow.Infrastructure.Tests.csproj package Testcontainers.PostgreSql

dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj package FluentAssertions
dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/HomeFlow.API.Tests/HomeFlow.API.Tests.csproj package Testcontainers.PostgreSql
```

- [ ] **Step 6: Delete auto-generated placeholder files**

Delete `Class1.cs` from Domain, Application, Infrastructure class libraries, and any `UnitTest1.cs` from test projects. Delete `WeatherForecast.cs` and `Controllers/WeatherForecastController.cs` from the API project if they exist.

- [ ] **Step 7: Verify solution builds**

```bash
dotnet build
```

Expected: Build succeeded with 0 errors.

- [ ] **Step 8: Commit**

```bash
git init
git add -A
git commit -m "chore: scaffold solution with Clean Architecture project structure"
```

---

## Task 2: Domain Enums

**Files:**
- Create: `src/HomeFlow.Domain/Enums/HouseholdTaskType.cs`
- Create: `src/HomeFlow.Domain/Enums/HouseholdTaskStatus.cs`

- [ ] **Step 1: Create HouseholdTaskType enum**

```csharp
// src/HomeFlow.Domain/Enums/HouseholdTaskType.cs
namespace HomeFlow.Domain.Enums;

public enum HouseholdTaskType
{
    OneOff = 0,
    Recurring = 1
}
```

- [ ] **Step 2: Create HouseholdTaskStatus enum**

```csharp
// src/HomeFlow.Domain/Enums/HouseholdTaskStatus.cs
namespace HomeFlow.Domain.Enums;

public enum HouseholdTaskStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Overdue = 3
}
```

- [ ] **Step 3: Verify build**

```bash
dotnet build src/HomeFlow.Domain/HomeFlow.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 4: Commit**

```bash
git add src/HomeFlow.Domain/Enums/
git commit -m "feat(domain): add HouseholdTaskType and HouseholdTaskStatus enums"
```

---

## Task 3: Domain Entities

**Files:**
- Create: `src/HomeFlow.Domain/Entities/User.cs`
- Create: `src/HomeFlow.Domain/Entities/HouseholdTask.cs`
- Create: `src/HomeFlow.Domain/Entities/RecurringTaskTemplate.cs`
- Create: `src/HomeFlow.Domain/Entities/RotationEntry.cs`

No dedicated tests — these are plain POCOs with no behavior. They get exercised through service and integration tests.

- [ ] **Step 1: Implement all entities**

```csharp
// src/HomeFlow.Domain/Entities/User.cs
namespace HomeFlow.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// src/HomeFlow.Domain/Entities/HouseholdTask.cs
using HomeFlow.Domain.Enums;

namespace HomeFlow.Domain.Entities;

public class HouseholdTask
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HouseholdTaskType TaskType { get; set; }
    public HouseholdTaskStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? TemplateId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
```

```csharp
// src/HomeFlow.Domain/Entities/RecurringTaskTemplate.cs
namespace HomeFlow.Domain.Entities;

public class RecurringTaskTemplate
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int FrequencyDays { get; set; }
    public int CurrentAssigneeIndex { get; set; }
    public DateTime? LastGeneratedDate { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

```csharp
// src/HomeFlow.Domain/Entities/RotationEntry.cs
namespace HomeFlow.Domain.Entities;

public class RotationEntry
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public int RotationOrder { get; set; }
}
```

- [ ] **Step 2: Verify build**

```bash
dotnet build
```

Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add src/HomeFlow.Domain/Entities/
git commit -m "feat(domain): add User, HouseholdTask, RecurringTaskTemplate, RotationEntry entities"
```

---

## Task 4: Domain Repository Interfaces & TaskFilter

**Files:**
- Create: `src/HomeFlow.Domain/Repositories/IUserRepository.cs`
- Create: `src/HomeFlow.Domain/Repositories/ITaskRepository.cs`
- Create: `src/HomeFlow.Domain/Repositories/IRecurringTaskTemplateRepository.cs`
- Create: `src/HomeFlow.Domain/Repositories/IRotationEntryRepository.cs`

- [ ] **Step 1: Create IUserRepository**

```csharp
// src/HomeFlow.Domain/Repositories/IUserRepository.cs
using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
}
```

- [ ] **Step 2: Create ITaskRepository with TaskFilter**

```csharp
// src/HomeFlow.Domain/Repositories/ITaskRepository.cs
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;

namespace HomeFlow.Domain.Repositories;

public record TaskFilter(
    Guid? AssignedToUserId,
    HouseholdTaskStatus? Status,
    HouseholdTaskType? TaskType
);

public interface ITaskRepository
{
    Task<HouseholdTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter);
    Task<HouseholdTask> CreateAsync(HouseholdTask task);
    Task<HouseholdTask> UpdateAsync(HouseholdTask task);
    Task DeleteAsync(Guid id);
}
```

- [ ] **Step 3: Create IRecurringTaskTemplateRepository**

```csharp
// src/HomeFlow.Domain/Repositories/IRecurringTaskTemplateRepository.cs
using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRecurringTaskTemplateRepository
{
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync();
    Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template);
    Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template);
    Task DeleteAsync(Guid id);
}
```

- [ ] **Step 4: Create IRotationEntryRepository**

```csharp
// src/HomeFlow.Domain/Repositories/IRotationEntryRepository.cs
using HomeFlow.Domain.Entities;

namespace HomeFlow.Domain.Repositories;

public interface IRotationEntryRepository
{
    Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId);
    Task CreateAsync(RotationEntry entry);
    Task DeleteByTemplateIdAsync(Guid templateId);
}
```

- [ ] **Step 5: Verify build**

```bash
dotnet build src/HomeFlow.Domain/HomeFlow.Domain.csproj
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/HomeFlow.Domain/Repositories/
git commit -m "feat(domain): add repository interfaces and TaskFilter record"
```

---

## Task 5: Application Layer — DTOs & Exceptions

**Files:**
- Create: `src/HomeFlow.Application/DTOs/Auth/RegisterRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/Auth/LoginRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/Auth/AuthResponse.cs`
- Create: `src/HomeFlow.Application/DTOs/Tasks/CreateTaskRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/Tasks/UpdateTaskRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/Tasks/TaskResponse.cs`
- Create: `src/HomeFlow.Application/DTOs/Tasks/TaskFilterDto.cs`
- Create: `src/HomeFlow.Application/DTOs/RecurringTasks/CreateRecurringTaskRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/RecurringTasks/UpdateRecurringTaskRequest.cs`
- Create: `src/HomeFlow.Application/DTOs/RecurringTasks/RecurringTaskResponse.cs`
- Create: `src/HomeFlow.Application/DTOs/Users/UserResponse.cs`
- Create: `src/HomeFlow.Application/Interfaces/IJwtTokenProvider.cs`
- Create: `src/HomeFlow.Application/Exceptions/ValidationException.cs`
- Create: `src/HomeFlow.Application/Exceptions/NotFoundException.cs`

- [ ] **Step 1: Create Auth DTOs**

```csharp
// src/HomeFlow.Application/DTOs/Auth/RegisterRequest.cs
namespace HomeFlow.Application.DTOs.Auth;

public record RegisterRequest(
    string Username,
    string Email,
    string Password,
    string DisplayName
);
```

```csharp
// src/HomeFlow.Application/DTOs/Auth/LoginRequest.cs
namespace HomeFlow.Application.DTOs.Auth;

public record LoginRequest(string Username, string Password);
```

```csharp
// src/HomeFlow.Application/DTOs/Auth/AuthResponse.cs
namespace HomeFlow.Application.DTOs.Auth;

public record AuthResponse(Guid UserId, string Username, string DisplayName, string Token);
```

- [ ] **Step 2: Create Task DTOs**

```csharp
// src/HomeFlow.Application/DTOs/Tasks/CreateTaskRequest.cs
namespace HomeFlow.Application.DTOs.Tasks;

public record CreateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToUserId
);
```

```csharp
// src/HomeFlow.Application/DTOs/Tasks/UpdateTaskRequest.cs
namespace HomeFlow.Application.DTOs.Tasks;

public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToUserId
);
```

```csharp
// src/HomeFlow.Application/DTOs/Tasks/TaskResponse.cs
using HomeFlow.Domain.Enums;

namespace HomeFlow.Application.DTOs.Tasks;

public record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    HouseholdTaskType TaskType,
    HouseholdTaskStatus Status,
    DateTime? DueDate,
    Guid? AssignedToUserId,
    Guid CreatedByUserId,
    Guid? TemplateId,
    DateTime CreatedAt,
    DateTime? CompletedAt
);
```

```csharp
// src/HomeFlow.Application/DTOs/Tasks/TaskFilterDto.cs
using HomeFlow.Domain.Enums;

namespace HomeFlow.Application.DTOs.Tasks;

public record TaskFilterDto(
    Guid? AssignedToUserId,
    HouseholdTaskStatus? Status,
    HouseholdTaskType? TaskType
);
```

- [ ] **Step 3: Create RecurringTask DTOs**

```csharp
// src/HomeFlow.Application/DTOs/RecurringTasks/CreateRecurringTaskRequest.cs
namespace HomeFlow.Application.DTOs.RecurringTasks;

public record CreateRecurringTaskRequest(
    string Title,
    string? Description,
    int FrequencyDays,
    List<Guid> UserIdsInOrder
);
```

```csharp
// src/HomeFlow.Application/DTOs/RecurringTasks/UpdateRecurringTaskRequest.cs
namespace HomeFlow.Application.DTOs.RecurringTasks;

public record UpdateRecurringTaskRequest(
    string Title,
    string? Description,
    int FrequencyDays,
    List<Guid>? UserIdsInOrder
);
```

```csharp
// src/HomeFlow.Application/DTOs/RecurringTasks/RecurringTaskResponse.cs
namespace HomeFlow.Application.DTOs.RecurringTasks;

public record RotationEntryResponse(Guid UserId, int RotationOrder);

public record RecurringTaskResponse(
    Guid Id,
    string Title,
    string? Description,
    int FrequencyDays,
    int CurrentAssigneeIndex,
    DateTime? LastGeneratedDate,
    DateTime CreatedAt,
    List<RotationEntryResponse> RotationEntries
);
```

- [ ] **Step 4: Create UserResponse DTO**

```csharp
// src/HomeFlow.Application/DTOs/Users/UserResponse.cs
namespace HomeFlow.Application.DTOs.Users;

public record UserResponse(Guid Id, string Username, string Email, string DisplayName, DateTime CreatedAt);
```

- [ ] **Step 5: Create IJwtTokenProvider interface**

```csharp
// src/HomeFlow.Application/Interfaces/IJwtTokenProvider.cs
using HomeFlow.Domain.Entities;

namespace HomeFlow.Application.Interfaces;

public interface IJwtTokenProvider
{
    string GenerateToken(User user);
}
```

- [ ] **Step 6: Create custom exceptions**

```csharp
// src/HomeFlow.Application/Exceptions/ValidationException.cs
namespace HomeFlow.Application.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
```

```csharp
// src/HomeFlow.Application/Exceptions/NotFoundException.cs
namespace HomeFlow.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}
```

- [ ] **Step 7: Verify build**

```bash
dotnet build src/HomeFlow.Application/HomeFlow.Application.csproj
```

Expected: Build succeeded.

- [ ] **Step 8: Commit**

```bash
git add src/HomeFlow.Application/
git commit -m "feat(application): add DTOs, IJwtTokenProvider, and custom exceptions"
```

---

## Task 6: Application Layer — UserService

**Files:**
- Create: `src/HomeFlow.Application/Services/UserService.cs`
- Create: `tests/HomeFlow.Application.Tests/Services/UserServiceTests.cs`

- [ ] **Step 1: Write UserService tests**

Focus: happy paths for register/login + one credential failure (proves BCrypt verification works).

```csharp
// tests/HomeFlow.Application.Tests/Services/UserServiceTests.cs
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
}
```

- [ ] **Step 2: Run tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "UserServiceTests"
```

Expected: FAIL — `UserService` class does not exist.

- [ ] **Step 3: Implement UserService**

```csharp
// src/HomeFlow.Application/Services/UserService.cs
using System.Text.RegularExpressions;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.DTOs.Users;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Interfaces;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public UserService(IUserRepository userRepository, IJwtTokenProvider jwtTokenProvider)
    {
        _userRepository = userRepository;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || request.Username.Length < 3 || request.Username.Length > 50)
            throw new ValidationException("Invalid username: must be between 3 and 50 characters.");

        if (string.IsNullOrWhiteSpace(request.Email) || !Regex.IsMatch(request.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ValidationException("Invalid email format.");

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ValidationException("Invalid password: must be at least 8 characters.");

        var existingByUsername = await _userRepository.GetByUsernameAsync(request.Username);
        if (existingByUsername is not null)
            throw new ValidationException("A user with this username already exists.");

        var existingByEmail = await _userRepository.GetByEmailAsync(request.Email);
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

        var created = await _userRepository.CreateAsync(user);
        var token = _jwtTokenProvider.GenerateToken(created);

        return new AuthResponse(created.Id, created.Username, created.DisplayName, token);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ValidationException("Invalid credentials.");

        var token = _jwtTokenProvider.GenerateToken(user);
        return new AuthResponse(user.Id, user.Username, user.DisplayName, token);
    }

    public async Task<UserResponse> GetByIdAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            throw new NotFoundException($"User with ID {userId} not found.");

        return new UserResponse(user.Id, user.Username, user.Email, user.DisplayName, user.CreatedAt);
    }
}
```

- [ ] **Step 4: Run tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "UserServiceTests"
```

Expected: All tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/HomeFlow.Application/Services/UserService.cs tests/HomeFlow.Application.Tests/Services/UserServiceTests.cs
git commit -m "feat(application): add UserService with registration, login, and validation"
```

---

## Task 7: Application Layer — TaskService

**Files:**
- Create: `src/HomeFlow.Application/Services/TaskService.cs`
- Create: `tests/HomeFlow.Application.Tests/Services/TaskServiceTests.cs`

- [ ] **Step 1: Write TaskService tests**

Focus: create happy path, overdue detection, completion rules (happy + already-completed + no-permission).

```csharp
// tests/HomeFlow.Application.Tests/Services/TaskServiceTests.cs
using FluentAssertions;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _taskRepository = Substitute.For<ITaskRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new TaskService(_taskRepository, _userRepository);
    }

    [Fact]
    public async Task CreateTask_ValidInput_ReturnsTaskResponse()
    {
        var createdBy = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();
        var dueDate = DateTime.UtcNow.AddDays(1);
        var request = new CreateTaskRequest("Buy groceries", "Weekly shopping", dueDate, assignedTo);

        _userRepository.GetByIdAsync(assignedTo).Returns(new User { Id = assignedTo });
        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });

        var result = await _sut.CreateTaskAsync(request, createdBy);

        result.Title.Should().Be("Buy groceries");
        result.TaskType.Should().Be(HouseholdTaskType.OneOff);
        result.Status.Should().Be(HouseholdTaskStatus.Pending);
        result.AssignedToUserId.Should().Be(assignedTo);
    }

    [Fact]
    public async Task GetAllTasks_OverdueTasks_StatusSetToOverdue()
    {
        var tasks = new List<HouseholdTask>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Overdue task",
                Status = HouseholdTaskStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(-1),
                CreatedByUserId = Guid.NewGuid()
            }
        };
        _taskRepository.GetAllAsync(Arg.Any<TaskFilter?>()).Returns(tasks);

        var result = (await _sut.GetAllTasksAsync(null)).ToList();

        result[0].Status.Should().Be(HouseholdTaskStatus.Overdue);
    }

    [Fact]
    public async Task CompleteTask_ValidRequest_SetsCompletedStatus()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Pending,
            AssignedToUserId = userId,
            CreatedByUserId = userId
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);
        _taskRepository.UpdateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo => callInfo.Arg<HouseholdTask>());

        var result = await _sut.CompleteTaskAsync(taskId, userId);

        result.Status.Should().Be(HouseholdTaskStatus.Completed);
        result.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteTask_AlreadyCompleted_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Completed,
            AssignedToUserId = userId,
            CreatedByUserId = userId
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);

        var act = () => _sut.CompleteTaskAsync(taskId, userId);

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already completed*");
    }

    [Fact]
    public async Task CompleteTask_NotAssignedOrCreator_ThrowsValidationException()
    {
        var taskId = Guid.NewGuid();
        var task = new HouseholdTask
        {
            Id = taskId,
            Title = "Test",
            Status = HouseholdTaskStatus.Pending,
            AssignedToUserId = Guid.NewGuid(),
            CreatedByUserId = Guid.NewGuid()
        };
        _taskRepository.GetByIdAsync(taskId).Returns(task);

        var act = () => _sut.CompleteTaskAsync(taskId, Guid.NewGuid());

        await act.Should().ThrowAsync<ValidationException>().WithMessage("*permission*");
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "TaskServiceTests"
```

Expected: FAIL — `TaskService` class does not exist.

- [ ] **Step 3: Implement TaskService**

```csharp
// src/HomeFlow.Application/Services/TaskService.cs
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class TaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
    {
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, Guid createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
            throw new ValidationException("Invalid due date: must be in the future.");

        if (request.AssignedToUserId.HasValue)
        {
            var assignedUser = await _userRepository.GetByIdAsync(request.AssignedToUserId.Value);
            if (assignedUser is null)
                throw new ValidationException("Invalid assigned user: user not found.");
        }

        var task = new HouseholdTask
        {
            Title = request.Title,
            Description = request.Description,
            TaskType = HouseholdTaskType.OneOff,
            Status = HouseholdTaskStatus.Pending,
            DueDate = request.DueDate,
            AssignedToUserId = request.AssignedToUserId,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _taskRepository.CreateAsync(task);
        return MapToResponse(created);
    }

    public async Task<IEnumerable<TaskResponse>> GetAllTasksAsync(TaskFilterDto? filter)
    {
        TaskFilter? domainFilter = filter is not null
            ? new TaskFilter(filter.AssignedToUserId, filter.Status, filter.TaskType)
            : null;

        var tasks = await _taskRepository.GetAllAsync(domainFilter);
        return tasks.Select(t => MapToResponse(FlagOverdue(t)));
    }

    public async Task<TaskResponse> GetTaskByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        return MapToResponse(FlagOverdue(task));
    }

    public async Task<TaskResponse> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.AssignedToUserId.HasValue)
        {
            var user = await _userRepository.GetByIdAsync(request.AssignedToUserId.Value);
            if (user is null)
                throw new ValidationException("Invalid assigned user: user not found.");
        }

        task.Title = request.Title;
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.AssignedToUserId = request.AssignedToUserId;

        var updated = await _taskRepository.UpdateAsync(task);
        return MapToResponse(updated);
    }

    public async Task<TaskResponse> CompleteTaskAsync(Guid id, Guid requestingUserId)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        if (task.Status == HouseholdTaskStatus.Completed)
            throw new ValidationException("Task is already completed.");

        if (task.AssignedToUserId != requestingUserId && task.CreatedByUserId != requestingUserId)
            throw new ValidationException("You do not have permission to complete this task.");

        task.Status = HouseholdTaskStatus.Completed;
        task.CompletedAt = DateTime.UtcNow;

        var updated = await _taskRepository.UpdateAsync(task);
        return MapToResponse(updated);
    }

    public async System.Threading.Tasks.Task DeleteTaskAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task is null)
            throw new NotFoundException($"Task with ID {id} not found.");

        await _taskRepository.DeleteAsync(id);
    }

    private static HouseholdTask FlagOverdue(HouseholdTask task)
    {
        if (task.DueDate.HasValue
            && task.DueDate.Value < DateTime.UtcNow
            && task.Status is HouseholdTaskStatus.Pending or HouseholdTaskStatus.InProgress)
        {
            task.Status = HouseholdTaskStatus.Overdue;
        }
        return task;
    }

    private static TaskResponse MapToResponse(HouseholdTask task)
    {
        return new TaskResponse(
            task.Id, task.Title, task.Description, task.TaskType, task.Status,
            task.DueDate, task.AssignedToUserId, task.CreatedByUserId,
            task.TemplateId, task.CreatedAt, task.CompletedAt
        );
    }
}
```

- [ ] **Step 4: Run tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "TaskServiceTests"
```

Expected: All tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/HomeFlow.Application/Services/TaskService.cs tests/HomeFlow.Application.Tests/Services/TaskServiceTests.cs
git commit -m "feat(application): add TaskService with CRUD, completion rules, and overdue detection"
```

---

## Task 8: Application Layer — RecurringTaskService

**Files:**
- Create: `src/HomeFlow.Application/Services/RecurringTaskService.cs`
- Create: `tests/HomeFlow.Application.Tests/Services/RecurringTaskServiceTests.cs`

- [ ] **Step 1: Write RecurringTaskService tests**

Focus: create template happy path, rotation logic (assign + advance + wrap), and not-found guard.

```csharp
// tests/HomeFlow.Application.Tests/Services/RecurringTaskServiceTests.cs
using FluentAssertions;
using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using NSubstitute;

namespace HomeFlow.Application.Tests.Services;

public class RecurringTaskServiceTests
{
    private readonly IRecurringTaskTemplateRepository _templateRepository;
    private readonly IRotationEntryRepository _rotationEntryRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;
    private readonly RecurringTaskService _sut;

    public RecurringTaskServiceTests()
    {
        _templateRepository = Substitute.For<IRecurringTaskTemplateRepository>();
        _rotationEntryRepository = Substitute.For<IRotationEntryRepository>();
        _taskRepository = Substitute.For<ITaskRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _sut = new RecurringTaskService(_templateRepository, _rotationEntryRepository, _taskRepository, _userRepository);
    }

    [Fact]
    public async Task CreateTemplate_ValidInput_CreatesTemplateAndRotationEntries()
    {
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var request = new CreateRecurringTaskRequest("Clean kitchen", "Deep clean", 7, new List<Guid> { user1, user2 });

        _userRepository.GetByIdAsync(user1).Returns(new User { Id = user1 });
        _userRepository.GetByIdAsync(user2).Returns(new User { Id = user2 });
        _templateRepository.CreateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<RecurringTaskTemplate>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _rotationEntryRepository.GetByTemplateIdAsync(Arg.Any<Guid>()).Returns(new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 }
        });

        var result = await _sut.CreateTemplateAsync(request);

        result.Title.Should().Be("Clean kitchen");
        result.FrequencyDays.Should().Be(7);
        result.RotationEntries.Should().HaveCount(2);
        await _rotationEntryRepository.Received(2).CreateAsync(Arg.Any<RotationEntry>());
    }

    [Fact]
    public async Task GenerateNextTask_AssignsToCurrentAndAdvancesIndex()
    {
        var templateId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        var user3 = Guid.NewGuid();

        var template = new RecurringTaskTemplate
        {
            Id = templateId,
            Title = "Clean kitchen",
            FrequencyDays = 7,
            CurrentAssigneeIndex = 1
        };
        _templateRepository.GetByIdAsync(templateId).Returns(template);

        var entries = new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 },
            new() { UserId = user3, RotationOrder = 2 }
        };
        _rotationEntryRepository.GetByTemplateIdAsync(templateId).Returns(entries);

        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _templateRepository.UpdateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo => callInfo.Arg<RecurringTaskTemplate>());

        var result = await _sut.GenerateNextTaskAsync(templateId);

        result.AssignedToUserId.Should().Be(user2);
        result.TaskType.Should().Be(HouseholdTaskType.Recurring);
        result.TemplateId.Should().Be(templateId);
        await _templateRepository.Received(1).UpdateAsync(Arg.Is<RecurringTaskTemplate>(t => t.CurrentAssigneeIndex == 2));
    }

    [Fact]
    public async Task GenerateNextTask_WrapsIndexAtEnd()
    {
        var templateId = Guid.NewGuid();
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var template = new RecurringTaskTemplate
        {
            Id = templateId,
            Title = "Trash",
            FrequencyDays = 3,
            CurrentAssigneeIndex = 1
        };
        _templateRepository.GetByIdAsync(templateId).Returns(template);

        var entries = new List<RotationEntry>
        {
            new() { UserId = user1, RotationOrder = 0 },
            new() { UserId = user2, RotationOrder = 1 }
        };
        _rotationEntryRepository.GetByTemplateIdAsync(templateId).Returns(entries);

        _taskRepository.CreateAsync(Arg.Any<HouseholdTask>()).Returns(callInfo =>
        {
            var t = callInfo.Arg<HouseholdTask>();
            t.Id = Guid.NewGuid();
            return t;
        });
        _templateRepository.UpdateAsync(Arg.Any<RecurringTaskTemplate>()).Returns(callInfo => callInfo.Arg<RecurringTaskTemplate>());

        var result = await _sut.GenerateNextTaskAsync(templateId);

        result.AssignedToUserId.Should().Be(user2);
        await _templateRepository.Received(1).UpdateAsync(Arg.Is<RecurringTaskTemplate>(t => t.CurrentAssigneeIndex == 0));
    }

    [Fact]
    public async Task GenerateNextTask_TemplateNotFound_ThrowsNotFoundException()
    {
        _templateRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((RecurringTaskTemplate?)null);

        var act = () => _sut.GenerateNextTaskAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
```

- [ ] **Step 2: Run tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "RecurringTaskServiceTests"
```

Expected: FAIL — `RecurringTaskService` class does not exist.

- [ ] **Step 3: Implement RecurringTaskService**

```csharp
// src/HomeFlow.Application/Services/RecurringTaskService.cs
using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Exceptions;
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;

namespace HomeFlow.Application.Services;

public class RecurringTaskService
{
    private readonly IRecurringTaskTemplateRepository _templateRepository;
    private readonly IRotationEntryRepository _rotationEntryRepository;
    private readonly ITaskRepository _taskRepository;
    private readonly IUserRepository _userRepository;

    public RecurringTaskService(
        IRecurringTaskTemplateRepository templateRepository,
        IRotationEntryRepository rotationEntryRepository,
        ITaskRepository taskRepository,
        IUserRepository userRepository)
    {
        _templateRepository = templateRepository;
        _rotationEntryRepository = rotationEntryRepository;
        _taskRepository = taskRepository;
        _userRepository = userRepository;
    }

    public async Task<RecurringTaskResponse> CreateTemplateAsync(CreateRecurringTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.FrequencyDays < 1)
            throw new ValidationException("Invalid frequency: must be at least 1 day.");

        if (request.UserIdsInOrder is null || request.UserIdsInOrder.Count == 0)
            throw new ValidationException("Invalid rotation: must include at least one user.");

        foreach (var userId in request.UserIdsInOrder)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
                throw new ValidationException($"Invalid user: user with ID {userId} not found.");
        }

        var template = new RecurringTaskTemplate
        {
            Title = request.Title,
            Description = request.Description,
            FrequencyDays = request.FrequencyDays,
            CurrentAssigneeIndex = 0,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _templateRepository.CreateAsync(template);

        for (var i = 0; i < request.UserIdsInOrder.Count; i++)
        {
            await _rotationEntryRepository.CreateAsync(new RotationEntry
            {
                TemplateId = created.Id,
                UserId = request.UserIdsInOrder[i],
                RotationOrder = i
            });
        }

        var entries = await _rotationEntryRepository.GetByTemplateIdAsync(created.Id);
        return MapToResponse(created, entries);
    }

    public async Task<IEnumerable<RecurringTaskResponse>> GetAllTemplatesAsync()
    {
        var templates = await _templateRepository.GetAllAsync();
        var results = new List<RecurringTaskResponse>();

        foreach (var template in templates)
        {
            var entries = await _rotationEntryRepository.GetByTemplateIdAsync(template.Id);
            results.Add(MapToResponse(template, entries));
        }

        return results;
    }

    public async Task<RecurringTaskResponse> GetTemplateByIdAsync(Guid id)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        var entries = await _rotationEntryRepository.GetByTemplateIdAsync(id);
        return MapToResponse(template, entries);
    }

    public async Task<RecurringTaskResponse> UpdateTemplateAsync(Guid id, UpdateRecurringTaskRequest request)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            throw new ValidationException("Invalid title: must be between 1 and 200 characters.");

        if (request.FrequencyDays < 1)
            throw new ValidationException("Invalid frequency: must be at least 1 day.");

        template.Title = request.Title;
        template.Description = request.Description;
        template.FrequencyDays = request.FrequencyDays;

        var updated = await _templateRepository.UpdateAsync(template);

        if (request.UserIdsInOrder is not null && request.UserIdsInOrder.Count > 0)
        {
            foreach (var userId in request.UserIdsInOrder)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user is null)
                    throw new ValidationException($"Invalid user: user with ID {userId} not found.");
            }

            await _rotationEntryRepository.DeleteByTemplateIdAsync(id);
            for (var i = 0; i < request.UserIdsInOrder.Count; i++)
            {
                await _rotationEntryRepository.CreateAsync(new RotationEntry
                {
                    TemplateId = id,
                    UserId = request.UserIdsInOrder[i],
                    RotationOrder = i
                });
            }

            updated.CurrentAssigneeIndex = 0;
            updated = await _templateRepository.UpdateAsync(updated);
        }

        var entries = await _rotationEntryRepository.GetByTemplateIdAsync(id);
        return MapToResponse(updated, entries);
    }

    public async System.Threading.Tasks.Task DeleteTemplateAsync(Guid id)
    {
        var template = await _templateRepository.GetByIdAsync(id);
        if (template is null)
            throw new NotFoundException($"Template with ID {id} not found.");

        await _templateRepository.DeleteAsync(id);
    }

    public async Task<TaskResponse> GenerateNextTaskAsync(Guid templateId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template is null)
            throw new NotFoundException($"Template with ID {templateId} not found.");

        var entries = (await _rotationEntryRepository.GetByTemplateIdAsync(templateId))
            .OrderBy(e => e.RotationOrder)
            .ToList();

        var currentEntry = entries[template.CurrentAssigneeIndex];

        var task = new HouseholdTask
        {
            Title = template.Title,
            Description = template.Description,
            TaskType = HouseholdTaskType.Recurring,
            Status = HouseholdTaskStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(template.FrequencyDays),
            AssignedToUserId = currentEntry.UserId,
            CreatedByUserId = currentEntry.UserId,
            TemplateId = templateId,
            CreatedAt = DateTime.UtcNow
        };

        var createdTask = await _taskRepository.CreateAsync(task);

        template.CurrentAssigneeIndex = (template.CurrentAssigneeIndex + 1) % entries.Count;
        template.LastGeneratedDate = DateTime.UtcNow;
        await _templateRepository.UpdateAsync(template);

        return new TaskResponse(
            createdTask.Id, createdTask.Title, createdTask.Description,
            createdTask.TaskType, createdTask.Status, createdTask.DueDate,
            createdTask.AssignedToUserId, createdTask.CreatedByUserId,
            createdTask.TemplateId, createdTask.CreatedAt, createdTask.CompletedAt
        );
    }

    private static RecurringTaskResponse MapToResponse(RecurringTaskTemplate template, IEnumerable<RotationEntry> entries)
    {
        return new RecurringTaskResponse(
            template.Id, template.Title, template.Description,
            template.FrequencyDays, template.CurrentAssigneeIndex,
            template.LastGeneratedDate, template.CreatedAt,
            entries.OrderBy(e => e.RotationOrder)
                .Select(e => new RotationEntryResponse(e.UserId, e.RotationOrder))
                .ToList()
        );
    }
}
```

- [ ] **Step 4: Run tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Application.Tests/ --filter "RecurringTaskServiceTests"
```

Expected: All tests PASS.

- [ ] **Step 5: Run all application tests**

```bash
dotnet test tests/HomeFlow.Application.Tests/
```

Expected: All tests PASS.

- [ ] **Step 6: Commit**

```bash
git add src/HomeFlow.Application/Services/RecurringTaskService.cs tests/HomeFlow.Application.Tests/Services/RecurringTaskServiceTests.cs
git commit -m "feat(application): add RecurringTaskService with rotation logic and task generation"
```

---

## Task 9: Infrastructure — Database Migrations

**Files:**
- Create: `src/HomeFlow.Infrastructure/Database/MigrationRunner.cs`
- Create: `src/HomeFlow.Infrastructure/Database/Migrations/001_CreateUsersTable.sql`
- Create: `src/HomeFlow.Infrastructure/Database/Migrations/002_CreateRecurringTaskTemplatesTable.sql`
- Create: `src/HomeFlow.Infrastructure/Database/Migrations/003_CreateHouseholdTasksTable.sql`
- Create: `src/HomeFlow.Infrastructure/Database/Migrations/004_CreateRotationEntriesTable.sql`
- Create: `src/HomeFlow.Infrastructure/Database/Migrations/005_SeedData.sql`
- Test: `tests/HomeFlow.Infrastructure.Tests/Database/MigrationRunnerTests.cs`

- [ ] **Step 1: Create SQL migration files**

```sql
-- src/HomeFlow.Infrastructure/Database/Migrations/001_CreateUsersTable.sql
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```

```sql
-- src/HomeFlow.Infrastructure/Database/Migrations/002_CreateRecurringTaskTemplatesTable.sql
CREATE TABLE IF NOT EXISTS recurring_task_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    frequency_days INT NOT NULL,
    current_assignee_index INT NOT NULL DEFAULT 0,
    last_generated_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);
```

```sql
-- src/HomeFlow.Infrastructure/Database/Migrations/003_CreateHouseholdTasksTable.sql
CREATE TABLE IF NOT EXISTS household_tasks (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    task_type SMALLINT NOT NULL,
    status SMALLINT NOT NULL DEFAULT 0,
    due_date TIMESTAMP,
    assigned_to_user_id UUID REFERENCES users(id),
    created_by_user_id UUID NOT NULL REFERENCES users(id),
    template_id UUID REFERENCES recurring_task_templates(id),
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMP
);
```

```sql
-- src/HomeFlow.Infrastructure/Database/Migrations/004_CreateRotationEntriesTable.sql
CREATE TABLE IF NOT EXISTS rotation_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_id UUID NOT NULL REFERENCES recurring_task_templates(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id),
    rotation_order INT NOT NULL
);
```

```sql
-- src/HomeFlow.Infrastructure/Database/Migrations/005_SeedData.sql
-- Seed 4 users (password: "Password123!" hashed with BCrypt)
INSERT INTO users (id, username, email, password_hash, display_name)
VALUES
    ('a1b2c3d4-0001-0000-0000-000000000001', 'pedro', 'pedro@homeflow.com',
     '$2a$11$K3rByEJkCgB6QHrT4GnvYOQGhTp6rYKsXNR5Qf1Fc0JjKMgPcLvGi', 'Pedro'),
    ('a1b2c3d4-0002-0000-0000-000000000002', 'maria', 'maria@homeflow.com',
     '$2a$11$K3rByEJkCgB6QHrT4GnvYOQGhTp6rYKsXNR5Qf1Fc0JjKMgPcLvGi', 'Maria'),
    ('a1b2c3d4-0003-0000-0000-000000000003', 'joao', 'joao@homeflow.com',
     '$2a$11$K3rByEJkCgB6QHrT4GnvYOQGhTp6rYKsXNR5Qf1Fc0JjKMgPcLvGi', 'João'),
    ('a1b2c3d4-0004-0000-0000-000000000004', 'ana', 'ana@homeflow.com',
     '$2a$11$K3rByEJkCgB6QHrT4GnvYOQGhTp6rYKsXNR5Qf1Fc0JjKMgPcLvGi', 'Ana')
ON CONFLICT (username) DO NOTHING;

-- Recurring template: Clean kitchen (weekly)
INSERT INTO recurring_task_templates (id, title, description, frequency_days, current_assignee_index)
VALUES
    ('b1b2c3d4-0001-0000-0000-000000000001', 'Clean kitchen', 'Deep clean the kitchen including counters, stove, and floor', 7, 0),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'Take out trash', 'Take all trash bags to the dumpster', 3, 0)
ON CONFLICT DO NOTHING;

-- Rotation entries for "Clean kitchen": Pedro → Maria → João → Ana
INSERT INTO rotation_entries (template_id, user_id, rotation_order)
VALUES
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0001-0000-0000-000000000001', 0),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0002-0000-0000-000000000002', 1),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0003-0000-0000-000000000003', 2),
    ('b1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0004-0000-0000-000000000004', 3)
ON CONFLICT DO NOTHING;

-- Rotation entries for "Take out trash": Ana → João → Maria → Pedro
INSERT INTO rotation_entries (template_id, user_id, rotation_order)
VALUES
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0004-0000-0000-000000000004', 0),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0003-0000-0000-000000000003', 1),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0002-0000-0000-000000000002', 2),
    ('b1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0001-0000-0000-000000000001', 3)
ON CONFLICT DO NOTHING;

-- One-off tasks
INSERT INTO household_tasks (title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id)
VALUES
    ('Buy groceries', 'Weekly grocery shopping at the supermarket', 0, 0,
     NOW() + INTERVAL '1 day', 'a1b2c3d4-0001-0000-0000-000000000001', 'a1b2c3d4-0001-0000-0000-000000000001'),
    ('Fix bathroom faucet', 'The faucet in the main bathroom is leaking', 0, 1,
     NOW() + INTERVAL '3 days', 'a1b2c3d4-0003-0000-0000-000000000003', 'a1b2c3d4-0001-0000-0000-000000000001'),
    ('Pay electricity bill', 'Monthly electricity bill payment', 0, 2,
     NULL, 'a1b2c3d4-0002-0000-0000-000000000002', 'a1b2c3d4-0002-0000-0000-000000000002')
ON CONFLICT DO NOTHING;
```

**Note on seed password hash:** The BCrypt hash above is a placeholder. During implementation, generate a proper hash for "Password123!" using BCrypt with work factor 11. You can generate it in C# with `BCrypt.Net.BCrypt.HashPassword("Password123!", 11)` and paste the result into the SQL file, OR use a separate seed-data setup step in the migration runner that hashes at runtime. The simpler approach is to pre-compute and hardcode the hash.

- [ ] **Step 2: Write MigrationRunner tests**

```csharp
// tests/HomeFlow.Infrastructure.Tests/Database/MigrationRunnerTests.cs
using FluentAssertions;
using HomeFlow.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeFlow.Infrastructure.Tests.Database;

public class MigrationRunnerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    [Fact]
    public async Task RunAsync_CreatesMigrationHistoryTable()
    {
        var runner = new MigrationRunner(_postgres.GetConnectionString());

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'migration_history'", conn);
        var result = await cmd.ExecuteScalarAsync();
        ((long)result!).Should().Be(1);
    }

    [Fact]
    public async Task RunAsync_RunsTwice_IsIdempotent()
    {
        var runner = new MigrationRunner(_postgres.GetConnectionString());

        await runner.RunAsync();
        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM migration_history", conn);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        count.Should().Be(5);
    }

    [Fact]
    public async Task RunAsync_CreatesAllTables()
    {
        var runner = new MigrationRunner(_postgres.GetConnectionString());

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        var tables = new[] { "users", "recurring_task_templates", "household_tasks", "rotation_entries" };
        foreach (var table in tables)
        {
            await using var cmd = new NpgsqlCommand(
                $"SELECT COUNT(*) FROM information_schema.tables WHERE table_name = '{table}'", conn);
            var result = (long)(await cmd.ExecuteScalarAsync())!;
            result.Should().Be(1, $"table '{table}' should exist");
        }
    }

    [Fact]
    public async Task RunAsync_SeedsData()
    {
        var runner = new MigrationRunner(_postgres.GetConnectionString());

        await runner.RunAsync();

        await using var conn = new NpgsqlConnection(_postgres.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users", conn);
        var userCount = (long)(await cmd.ExecuteScalarAsync())!;
        userCount.Should().Be(4);
    }
}
```

- [ ] **Step 3: Run tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "MigrationRunnerTests"
```

Expected: FAIL — `MigrationRunner` class does not exist.

- [ ] **Step 4: Implement MigrationRunner**

The csproj for Infrastructure needs to embed the SQL files. Add to `HomeFlow.Infrastructure.csproj`:

```xml
<ItemGroup>
    <EmbeddedResource Include="Database\Migrations\*.sql" />
</ItemGroup>
```

```csharp
// src/HomeFlow.Infrastructure/Database/MigrationRunner.cs
using System.Reflection;
using Npgsql;

namespace HomeFlow.Infrastructure.Database;

public class MigrationRunner
{
    private readonly string _connectionString;

    public MigrationRunner(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task RunAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await CreateMigrationHistoryTableAsync(connection);

        var migrations = GetMigrationFiles();
        foreach (var (name, sql) in migrations)
        {
            if (await HasBeenAppliedAsync(connection, name))
                continue;

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                await cmd.ExecuteNonQueryAsync();

                await using var recordCmd = new NpgsqlCommand(
                    "INSERT INTO migration_history (migration_name) VALUES (@name)", connection, transaction);
                recordCmd.Parameters.AddWithValue("name", name);
                await recordCmd.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }

    private static async Task CreateMigrationHistoryTableAsync(NpgsqlConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS migration_history (
                id SERIAL PRIMARY KEY,
                migration_name VARCHAR(255) UNIQUE NOT NULL,
                applied_at TIMESTAMP NOT NULL DEFAULT NOW()
            )
            """;
        await using var cmd = new NpgsqlCommand(sql, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<bool> HasBeenAppliedAsync(NpgsqlConnection connection, string migrationName)
    {
        await using var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM migration_history WHERE migration_name = @name", connection);
        cmd.Parameters.AddWithValue("name", migrationName);
        var count = (long)(await cmd.ExecuteScalarAsync())!;
        return count > 0;
    }

    private static List<(string Name, string Sql)> GetMigrationFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = "HomeFlow.Infrastructure.Database.Migrations.";

        return assembly.GetManifestResourceNames()
            .Where(n => n.StartsWith(prefix) && n.EndsWith(".sql"))
            .OrderBy(n => n)
            .Select(resourceName =>
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream);
                var sql = reader.ReadToEnd();
                var name = resourceName[prefix.Length..];
                return (name, sql);
            })
            .ToList();
    }
}
```

- [ ] **Step 5: Run tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "MigrationRunnerTests"
```

Expected: All tests PASS.

- [ ] **Step 6: Commit**

```bash
git add src/HomeFlow.Infrastructure/Database/ src/HomeFlow.Infrastructure/HomeFlow.Infrastructure.csproj tests/HomeFlow.Infrastructure.Tests/Database/
git commit -m "feat(infrastructure): add MigrationRunner with SQL migrations and seed data"
```

---

## Task 10: Infrastructure — Repositories

**Files:**
- Create: `src/HomeFlow.Infrastructure/Repositories/UserRepository.cs`
- Create: `src/HomeFlow.Infrastructure/Repositories/TaskRepository.cs`
- Create: `src/HomeFlow.Infrastructure/Repositories/RecurringTaskTemplateRepository.cs`
- Create: `src/HomeFlow.Infrastructure/Repositories/RotationEntryRepository.cs`
- Test: `tests/HomeFlow.Infrastructure.Tests/Repositories/DatabaseFixture.cs`
- Test: `tests/HomeFlow.Infrastructure.Tests/Repositories/UserRepositoryTests.cs`

Only UserRepository gets dedicated integration tests — it proves the Npgsql mapping pattern works. The other repositories follow the same pattern and get exercised through API integration tests in Task 14.

- [ ] **Step 1: Create shared PostgreSQL test fixture**

```csharp
// tests/HomeFlow.Infrastructure.Tests/Repositories/DatabaseFixture.cs
using HomeFlow.Infrastructure.Database;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HomeFlow.Infrastructure.Tests.Repositories;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var runner = new MigrationRunner(ConnectionString);
        await runner.RunAsync();
    }

    public async Task DisposeAsync() => await _postgres.DisposeAsync();

    public async Task CleanTablesAsync()
    {
        await using var conn = new NpgsqlConnection(ConnectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM household_tasks; DELETE FROM rotation_entries; DELETE FROM recurring_task_templates; DELETE FROM users;",
            conn);
        await cmd.ExecuteNonQueryAsync();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
```

- [ ] **Step 2: Write UserRepository tests**

```csharp
// tests/HomeFlow.Infrastructure.Tests/Repositories/UserRepositoryTests.cs
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
        _sut = new UserRepository(fixture.ConnectionString);
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
```

- [ ] **Step 3: Run UserRepository tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "UserRepositoryTests"
```

Expected: FAIL — `UserRepository` class does not exist.

- [ ] **Step 4: Implement UserRepository**

```csharp
// src/HomeFlow.Infrastructure/Repositories/UserRepository.cs
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public UserRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await ReadUserAsync(cmd);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE username = @username", conn);
        cmd.Parameters.AddWithValue("username", username);
        return await ReadUserAsync(cmd);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, username, email, password_hash, display_name, created_at FROM users WHERE email = @email", conn);
        cmd.Parameters.AddWithValue("email", email);
        return await ReadUserAsync(cmd);
    }

    public async Task<User> CreateAsync(User user)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO users (username, email, password_hash, display_name, created_at)
            VALUES (@username, @email, @passwordHash, @displayName, @createdAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("username", user.Username);
        cmd.Parameters.AddWithValue("email", user.Email);
        cmd.Parameters.AddWithValue("passwordHash", user.PasswordHash);
        cmd.Parameters.AddWithValue("displayName", user.DisplayName);
        cmd.Parameters.AddWithValue("createdAt", user.CreatedAt);

        user.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return user;
    }

    private static async Task<User?> ReadUserAsync(NpgsqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new User
        {
            Id = reader.GetGuid(0),
            Username = reader.GetString(1),
            Email = reader.GetString(2),
            PasswordHash = reader.GetString(3),
            DisplayName = reader.GetString(4),
            CreatedAt = reader.GetDateTime(5)
        };
    }
}
```

- [ ] **Step 5: Run UserRepository tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "UserRepositoryTests"
```

Expected: All tests PASS.

- [ ] **Step 6: Implement TaskRepository**

```csharp
// src/HomeFlow.Infrastructure/Repositories/TaskRepository.cs
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Enums;
using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly string _connectionString;

    public TaskRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<HouseholdTask?> GetByIdAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at FROM household_tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        return await ReadTaskAsync(cmd);
    }

    public async Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        var sql = "SELECT id, title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at FROM household_tasks WHERE 1=1";
        var parameters = new List<NpgsqlParameter>();

        if (filter?.AssignedToUserId.HasValue == true)
        {
            sql += " AND assigned_to_user_id = @assignedTo";
            parameters.Add(new NpgsqlParameter("assignedTo", filter.AssignedToUserId.Value));
        }
        if (filter?.Status.HasValue == true)
        {
            sql += " AND status = @status";
            parameters.Add(new NpgsqlParameter("status", (short)filter.Status.Value));
        }
        if (filter?.TaskType.HasValue == true)
        {
            sql += " AND task_type = @taskType";
            parameters.Add(new NpgsqlParameter("taskType", (short)filter.TaskType.Value));
        }

        sql += " ORDER BY created_at DESC";

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        var tasks = new List<HouseholdTask>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            tasks.Add(MapFromReader(reader));
        }
        return tasks;
    }

    public async Task<HouseholdTask> CreateAsync(HouseholdTask task)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO household_tasks (title, description, task_type, status, due_date, assigned_to_user_id, created_by_user_id, template_id, created_at, completed_at)
            VALUES (@title, @description, @taskType, @status, @dueDate, @assignedTo, @createdBy, @templateId, @createdAt, @completedAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("taskType", (short)task.TaskType);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("assignedTo", (object?)task.AssignedToUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdBy", task.CreatedByUserId);
        cmd.Parameters.AddWithValue("templateId", (object?)task.TemplateId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdAt", task.CreatedAt);
        cmd.Parameters.AddWithValue("completedAt", (object?)task.CompletedAt ?? DBNull.Value);

        task.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return task;
    }

    public async Task<HouseholdTask> UpdateAsync(HouseholdTask task)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE household_tasks
            SET title = @title, description = @description, status = @status,
                due_date = @dueDate, assigned_to_user_id = @assignedTo, completed_at = @completedAt
            WHERE id = @id
            """, conn);
        cmd.Parameters.AddWithValue("id", task.Id);
        cmd.Parameters.AddWithValue("title", task.Title);
        cmd.Parameters.AddWithValue("description", (object?)task.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("status", (short)task.Status);
        cmd.Parameters.AddWithValue("dueDate", (object?)task.DueDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("assignedTo", (object?)task.AssignedToUserId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("completedAt", (object?)task.CompletedAt ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return task;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM household_tasks WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static async Task<HouseholdTask?> ReadTaskAsync(NpgsqlCommand cmd)
    {
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapFromReader(reader);
    }

    private static HouseholdTask MapFromReader(NpgsqlDataReader reader)
    {
        return new HouseholdTask
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            TaskType = (HouseholdTaskType)reader.GetInt16(3),
            Status = (HouseholdTaskStatus)reader.GetInt16(4),
            DueDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            AssignedToUserId = reader.IsDBNull(6) ? null : reader.GetGuid(6),
            CreatedByUserId = reader.GetGuid(7),
            TemplateId = reader.IsDBNull(8) ? null : reader.GetGuid(8),
            CreatedAt = reader.GetDateTime(9),
            CompletedAt = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
        };
    }
}
```

- [ ] **Step 7: Implement RecurringTaskTemplateRepository**

```csharp
// src/HomeFlow.Infrastructure/Repositories/RecurringTaskTemplateRepository.cs
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RecurringTaskTemplateRepository : IRecurringTaskTemplateRepository
{
    private readonly string _connectionString;

    public RecurringTaskTemplateRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<RecurringTaskTemplate?> GetByIdAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return MapFromReader(reader);
    }

    public async Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync()
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, frequency_days, current_assignee_index, last_generated_date, created_at FROM recurring_task_templates ORDER BY created_at DESC", conn);
        var results = new List<RecurringTaskTemplate>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            results.Add(MapFromReader(reader));
        return results;
    }

    public async Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO recurring_task_templates (title, description, frequency_days, current_assignee_index, last_generated_date, created_at)
            VALUES (@title, @description, @frequencyDays, @currentAssigneeIndex, @lastGeneratedDate, @createdAt)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);
        cmd.Parameters.AddWithValue("createdAt", template.CreatedAt);

        template.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
        return template;
    }

    public async Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            UPDATE recurring_task_templates
            SET title = @title, description = @description, frequency_days = @frequencyDays,
                current_assignee_index = @currentAssigneeIndex, last_generated_date = @lastGeneratedDate
            WHERE id = @id
            """, conn);
        cmd.Parameters.AddWithValue("id", template.Id);
        cmd.Parameters.AddWithValue("title", template.Title);
        cmd.Parameters.AddWithValue("description", (object?)template.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("frequencyDays", template.FrequencyDays);
        cmd.Parameters.AddWithValue("currentAssigneeIndex", template.CurrentAssigneeIndex);
        cmd.Parameters.AddWithValue("lastGeneratedDate", (object?)template.LastGeneratedDate ?? DBNull.Value);

        await cmd.ExecuteNonQueryAsync();
        return template;
    }

    public async System.Threading.Tasks.Task DeleteAsync(Guid id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("DELETE FROM recurring_task_templates WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    private static RecurringTaskTemplate MapFromReader(NpgsqlDataReader reader)
    {
        return new RecurringTaskTemplate
        {
            Id = reader.GetGuid(0),
            Title = reader.GetString(1),
            Description = reader.IsDBNull(2) ? null : reader.GetString(2),
            FrequencyDays = reader.GetInt32(3),
            CurrentAssigneeIndex = reader.GetInt32(4),
            LastGeneratedDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
            CreatedAt = reader.GetDateTime(6)
        };
    }
}
```

- [ ] **Step 8: Implement RotationEntryRepository**

```csharp
// src/HomeFlow.Infrastructure/Repositories/RotationEntryRepository.cs
using HomeFlow.Domain.Entities;
using HomeFlow.Domain.Repositories;
using Npgsql;

namespace HomeFlow.Infrastructure.Repositories;

public class RotationEntryRepository : IRotationEntryRepository
{
    private readonly string _connectionString;

    public RotationEntryRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IEnumerable<RotationEntry>> GetByTemplateIdAsync(Guid templateId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT id, template_id, user_id, rotation_order FROM rotation_entries WHERE template_id = @templateId ORDER BY rotation_order", conn);
        cmd.Parameters.AddWithValue("templateId", templateId);

        var results = new List<RotationEntry>();
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new RotationEntry
            {
                Id = reader.GetGuid(0),
                TemplateId = reader.GetGuid(1),
                UserId = reader.GetGuid(2),
                RotationOrder = reader.GetInt32(3)
            });
        }
        return results;
    }

    public async System.Threading.Tasks.Task CreateAsync(RotationEntry entry)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            """
            INSERT INTO rotation_entries (template_id, user_id, rotation_order)
            VALUES (@templateId, @userId, @rotationOrder)
            RETURNING id
            """, conn);
        cmd.Parameters.AddWithValue("templateId", entry.TemplateId);
        cmd.Parameters.AddWithValue("userId", entry.UserId);
        cmd.Parameters.AddWithValue("rotationOrder", entry.RotationOrder);

        entry.Id = (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async System.Threading.Tasks.Task DeleteByTemplateIdAsync(Guid templateId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM rotation_entries WHERE template_id = @templateId", conn);
        cmd.Parameters.AddWithValue("templateId", templateId);
        await cmd.ExecuteNonQueryAsync();
    }
}
```

- [ ] **Step 9: Run UserRepository tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "UserRepositoryTests"
```

Expected: All tests PASS.

- [ ] **Step 10: Verify build**

```bash
dotnet build
```

Expected: Build succeeded.

- [ ] **Step 11: Commit**

```bash
git add src/HomeFlow.Infrastructure/Repositories/ tests/HomeFlow.Infrastructure.Tests/Repositories/
git commit -m "feat(infrastructure): add Npgsql repositories with UserRepository integration tests"
```

---

## Task 11: Infrastructure — JWT Token Provider

**Files:**
- Create: `src/HomeFlow.Infrastructure/Auth/JwtTokenProvider.cs`
- Test: `tests/HomeFlow.Infrastructure.Tests/Auth/JwtTokenProviderTests.cs`

- [ ] **Step 1: Write JwtTokenProvider tests**

```csharp
// tests/HomeFlow.Infrastructure.Tests/Auth/JwtTokenProviderTests.cs
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
```

- [ ] **Step 2: Run tests — verify they fail**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "JwtTokenProviderTests"
```

Expected: FAIL — `JwtTokenProvider` class does not exist.

- [ ] **Step 3: Implement JwtTokenProvider**

```csharp
// src/HomeFlow.Infrastructure/Auth/JwtTokenProvider.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HomeFlow.Application.Interfaces;
using HomeFlow.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace HomeFlow.Infrastructure.Auth;

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenProvider(string key, string issuer, string audience, int expirationMinutes)
    {
        _key = key;
        _issuer = issuer;
        _audience = audience;
        _expirationMinutes = expirationMinutes;
    }

    public string GenerateToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Step 4: Run tests — verify they pass**

```bash
dotnet test tests/HomeFlow.Infrastructure.Tests/ --filter "JwtTokenProviderTests"
```

Expected: All tests PASS.

- [ ] **Step 5: Commit**

```bash
git add src/HomeFlow.Infrastructure/Auth/ tests/HomeFlow.Infrastructure.Tests/Auth/
git commit -m "feat(infrastructure): add JwtTokenProvider with HMAC-SHA256 signing"
```

---

## Task 12: API Layer — Program.cs, Middleware, Configuration

**Files:**
- Modify: `src/HomeFlow.API/Program.cs`
- Create: `src/HomeFlow.API/Middleware/ExceptionHandlingMiddleware.cs`
- Modify: `src/HomeFlow.API/appsettings.json`
- Create: `src/HomeFlow.API/appsettings.Development.json`

- [ ] **Step 1: Create ExceptionHandlingMiddleware**

```csharp
// src/HomeFlow.API/Middleware/ExceptionHandlingMiddleware.cs
using System.Text.Json;
using HomeFlow.Application.Exceptions;

namespace HomeFlow.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (Exception)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = "An unexpected error occurred." }));
        }
    }
}
```

- [ ] **Step 2: Configure appsettings**

```json
// src/HomeFlow.API/appsettings.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Jwt": {
    "Key": "CHANGE-THIS-TO-A-STRONG-SECRET-KEY-AT-LEAST-32-BYTES-LONG",
    "Issuer": "HomeFlow",
    "Audience": "HomeFlow",
    "ExpirationMinutes": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=homeflow;Username=homeflow;Password=homeflow_dev"
  }
}
```

```json
// src/HomeFlow.API/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
```

- [ ] **Step 3: Write Program.cs with DI, auth, CORS, and middleware**

```csharp
// src/HomeFlow.API/Program.cs
using System.Text;
using HomeFlow.API.Middleware;
using HomeFlow.Application.Interfaces;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Repositories;
using HomeFlow.Infrastructure.Auth;
using HomeFlow.Infrastructure.Database;
using HomeFlow.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var jwtConfig = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtConfig["Key"]!;
var jwtIssuer = jwtConfig["Issuer"]!;
var jwtAudience = jwtConfig["Audience"]!;
var jwtExpiration = int.Parse(jwtConfig["ExpirationMinutes"]!);

builder.Services.AddSingleton(new MigrationRunner(connectionString));

builder.Services.AddScoped<IUserRepository>(_ => new UserRepository(connectionString));
builder.Services.AddScoped<ITaskRepository>(_ => new TaskRepository(connectionString));
builder.Services.AddScoped<IRecurringTaskTemplateRepository>(_ => new RecurringTaskTemplateRepository(connectionString));
builder.Services.AddScoped<IRotationEntryRepository>(_ => new RotationEntryRepository(connectionString));

builder.Services.AddScoped<IJwtTokenProvider>(_ => new JwtTokenProvider(jwtKey, jwtIssuer, jwtAudience, jwtExpiration));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<RecurringTaskService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var migrationRunner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await migrationRunner.RunAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
```

- [ ] **Step 4: Verify build**

```bash
dotnet build src/HomeFlow.API/HomeFlow.API.csproj
```

Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add src/HomeFlow.API/
git commit -m "feat(api): configure Program.cs with DI, JWT auth, CORS, and exception middleware"
```

---

## Task 13: API Layer — Controllers

**Files:**
- Create: `src/HomeFlow.API/Controllers/HealthController.cs`
- Create: `src/HomeFlow.API/Controllers/AuthController.cs`
- Create: `src/HomeFlow.API/Controllers/TasksController.cs`
- Create: `src/HomeFlow.API/Controllers/RecurringTasksController.cs`

- [ ] **Step 1: Create HealthController**

```csharp
// src/HomeFlow.API/Controllers/HealthController.cs
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
```

- [ ] **Step 2: Create AuthController**

```csharp
// src/HomeFlow.API/Controllers/AuthController.cs
using System.Security.Claims;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _userService.RegisterAsync(request);
        return Created($"/api/auth/me", result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _userService.LoginAsync(request);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _userService.GetByIdAsync(userId);
        return Ok(result);
    }
}
```

- [ ] **Step 3: Create TasksController**

```csharp
// src/HomeFlow.API/Controllers/TasksController.cs
using System.Security.Claims;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.Services;
using HomeFlow.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly TaskService _taskService;

    public TasksController(TaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? assignedToUserId,
        [FromQuery] HouseholdTaskStatus? status,
        [FromQuery] HouseholdTaskType? taskType)
    {
        TaskFilterDto? filter = null;
        if (assignedToUserId.HasValue || status.HasValue || taskType.HasValue)
            filter = new TaskFilterDto(assignedToUserId, status, taskType);

        var result = await _taskService.GetAllTasksAsync(filter);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _taskService.GetTaskByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.CreateTaskAsync(request, userId);
        return Created($"/api/tasks/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskRequest request)
    {
        var result = await _taskService.UpdateTaskAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _taskService.DeleteTaskAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var userId = GetCurrentUserId();
        var result = await _taskService.CompleteTaskAsync(id, userId);
        return Ok(result);
    }

    private Guid GetCurrentUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
```

- [ ] **Step 4: Create RecurringTasksController**

```csharp
// src/HomeFlow.API/Controllers/RecurringTasksController.cs
using HomeFlow.Application.DTOs.RecurringTasks;
using HomeFlow.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeFlow.API.Controllers;

[ApiController]
[Route("api/recurring-tasks")]
[Authorize]
public class RecurringTasksController : ControllerBase
{
    private readonly RecurringTaskService _recurringTaskService;

    public RecurringTasksController(RecurringTaskService recurringTaskService)
    {
        _recurringTaskService = recurringTaskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _recurringTaskService.GetAllTemplatesAsync();
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _recurringTaskService.GetTemplateByIdAsync(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecurringTaskRequest request)
    {
        var result = await _recurringTaskService.CreateTemplateAsync(request);
        return Created($"/api/recurring-tasks/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecurringTaskRequest request)
    {
        var result = await _recurringTaskService.UpdateTemplateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _recurringTaskService.DeleteTemplateAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/generate")]
    public async Task<IActionResult> GenerateNext(Guid id)
    {
        var result = await _recurringTaskService.GenerateNextTaskAsync(id);
        return Created($"/api/tasks/{result.Id}", result);
    }
}
```

- [ ] **Step 5: Verify build**

```bash
dotnet build src/HomeFlow.API/HomeFlow.API.csproj
```

Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add src/HomeFlow.API/Controllers/
git commit -m "feat(api): add Auth, Tasks, RecurringTasks, and Health controllers"
```

---

## Task 14: API Integration Tests

**Files:**
- Create: `tests/HomeFlow.API.Tests/CustomWebApplicationFactory.cs`
- Create: `tests/HomeFlow.API.Tests/IntegrationTests.cs`

4 focused integration tests that prove the entire stack works end-to-end: auth pipeline, task CRUD + completion, and rotation logic.

- [ ] **Step 1: Create CustomWebApplicationFactory**

```csharp
// tests/HomeFlow.API.Tests/CustomWebApplicationFactory.cs
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace HomeFlow.API.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .Build();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("Jwt:Key", "test-secret-key-for-integration-tests-at-least-32-bytes-long!!");
        builder.UseSetting("Jwt:Issuer", "HomeFlow");
        builder.UseSetting("Jwt:Audience", "HomeFlow");
        builder.UseSetting("Jwt:ExpirationMinutes", "60");
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```

- [ ] **Step 2: Write integration tests**

```csharp
// tests/HomeFlow.API.Tests/IntegrationTests.cs
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HomeFlow.Application.DTOs.Auth;
using HomeFlow.Application.DTOs.Tasks;
using HomeFlow.Application.DTOs.RecurringTasks;

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
        // Register
        var registerRequest = new RegisterRequest("authtest", "authtest@test.com", "Password123!", "Auth Test");
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var registerContent = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = registerContent.GetProperty("token").GetString()!;
        token.Should().NotBeNullOrEmpty();

        // Login
        var loginRequest = new LoginRequest("authtest", "Password123!");
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Access /me with token
        SetAuth(token);
        var meResponse = await _client.GetAsync("/api/auth/me");
        meResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var meContent = await meResponse.Content.ReadFromJsonAsync<JsonElement>();
        meContent.GetProperty("username").GetString().Should().Be("authtest");

        // Access /me without token → 401
        _client.DefaultRequestHeaders.Authorization = null;
        var unauthResponse = await _client.GetAsync("/api/auth/me");
        unauthResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TaskFlow_CreateCompleteAndVerifyStatus()
    {
        var (token, _) = await RegisterAndGetTokenAsync("taskflow");
        SetAuth(token);

        // Create task
        var createRequest = new CreateTaskRequest("Integration test task", null, DateTime.UtcNow.AddDays(1), null);
        var createResponse = await _client.PostAsJsonAsync("/api/tasks", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var taskId = created.GetProperty("id").GetString()!;

        // Complete task
        var completeResponse = await _client.PatchAsync($"/api/tasks/{taskId}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completed = await completeResponse.Content.ReadFromJsonAsync<JsonElement>();
        completed.GetProperty("status").GetInt32().Should().Be(2); // Completed
        completed.GetProperty("completedAt").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecurringFlow_CreateTemplateGenerateAndVerifyRotation()
    {
        var (token1, userId1) = await RegisterAndGetTokenAsync("rot1");
        var (_, userId2) = await RegisterAndGetTokenAsync("rot2");
        SetAuth(token1);

        // Create template with 2-member rotation
        var createRequest = new CreateRecurringTaskRequest("Rotation test", null, 3, new List<Guid> { userId1, userId2 });
        var createResponse = await _client.PostAsJsonAsync("/api/recurring-tasks", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var templateId = created.GetProperty("id").GetString()!;

        // Generate first task → assigned to userId1 (index 0)
        var gen1 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        gen1.StatusCode.Should().Be(HttpStatusCode.Created);
        var task1 = await gen1.Content.ReadFromJsonAsync<JsonElement>();
        task1.GetProperty("assignedToUserId").GetString().Should().Be(userId1.ToString());
        task1.GetProperty("taskType").GetInt32().Should().Be(1); // Recurring

        // Generate second task → assigned to userId2 (index 1, rotation advanced)
        var gen2 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        var task2 = await gen2.Content.ReadFromJsonAsync<JsonElement>();
        task2.GetProperty("assignedToUserId").GetString().Should().Be(userId2.ToString());

        // Generate third task → wraps back to userId1 (index 0)
        var gen3 = await _client.PostAsync($"/api/recurring-tasks/{templateId}/generate", null);
        var task3 = await gen3.Content.ReadFromJsonAsync<JsonElement>();
        task3.GetProperty("assignedToUserId").GetString().Should().Be(userId1.ToString());
    }
}
```

- [ ] **Step 3: Run all API tests**

```bash
dotnet test tests/HomeFlow.API.Tests/
```

Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add tests/HomeFlow.API.Tests/
git commit -m "feat(api): add focused integration tests covering auth, tasks, and rotation"
```

---

## Task 15: Final Verification

- [ ] **Step 1: Run entire test suite**

```bash
dotnet test HomeFlow.sln
```

Expected: All tests across all 3 test projects PASS (Application, Infrastructure, API).

- [ ] **Step 2: Verify build**

```bash
dotnet build HomeFlow.sln
```

Expected: Build succeeded with 0 errors.

- [ ] **Step 3: Commit any remaining changes**

```bash
git status
```

If there are uncommitted changes, commit them with an appropriate message.

- [ ] **Step 4: Final commit (if needed)**

```bash
git log --oneline
```

Verify the commit history is clean and tells a clear story of the build-up.

---

## Appendix: Docker & CI (Quick Reference)

These are covered in a separate DevOps plan. Key notes for the backend:

**Dockerfile** (for `src/`):
- Multi-stage build: `dotnet publish` → runtime image
- Expose port 8080
- Entrypoint: `dotnet HomeFlow.API.dll`

**docker-compose.yml**:
- PostgreSQL 16 on port 5432
- API on port 5000 → 8080
- Connection string via environment variable

**CI** (`.github/workflows/ci.yml`):
- PostgreSQL service container
- `dotnet restore` → `dotnet build` → `dotnet test`
