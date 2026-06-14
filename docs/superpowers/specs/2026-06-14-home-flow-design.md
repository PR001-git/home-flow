# HomeFlow — Design Specification

## User Story

> As a household member, I want to manage and fairly distribute household tasks among all members, so that chores are rotated automatically and one-off tasks can be assigned with deadlines, ensuring nothing is forgotten and no one is overburdened.

## Overview

HomeFlow is a household task management app for 4 members in a single household. It supports:

- **Recurring chores** with automatic rotation (e.g., cleaning kitchen rotates weekly among all members)
- **One-off tasks** with manual assignment and due dates (e.g., "buy groceries by Friday")
- **Accountability** via status tracking, overdue detection, and a dashboard showing task distribution

## Technical Constraints (Interview Exercise)

- .NET C# / ASP.NET Web API
- Clean Architecture (strict layer separation)
- No Entity Framework, Dapper, or Mediator
- PostgreSQL with raw Npgsql
- TDD with full test coverage
- React + TypeScript frontend
- Seeded data for demo

---

## Architecture

### Clean Architecture Layers

```
API → Application → Domain ← Infrastructure
```

- **Domain** — entities, enums, repository interfaces. No dependencies.
- **Application** — business services, DTOs, validation. Depends on Domain.
- **Infrastructure** — Npgsql repositories, JWT provider, migration runner. Implements Domain interfaces.
- **API** — ASP.NET controllers, middleware, DI configuration. Depends on Application + Infrastructure (for wiring).

### Project Structure

```
HomeFlow.sln
├── src/
│   ├── HomeFlow.Domain/
│   ├── HomeFlow.Application/
│   ├── HomeFlow.Infrastructure/
│   │   └── Database/
│   │       ├── MigrationRunner.cs
│   │       └── Migrations/
│   │           ├── 001_CreateUsersTable.sql
│   │           ├── 002_CreateRecurringTaskTemplatesTable.sql
│   │           ├── 003_CreateHouseholdTasksTable.sql
│   │           ├── 004_CreateRotationEntriesTable.sql
│   │           └── 005_SeedData.sql
│   └── HomeFlow.API/
├── home-flow-ui/                    # React + TypeScript (Vite)
├── tests/
│   ├── HomeFlow.Domain.Tests/
│   ├── HomeFlow.Application.Tests/
│   ├── HomeFlow.Infrastructure.Tests/
│   └── HomeFlow.API.Tests/
├── docker-compose.yml
├── .github/workflows/ci.yml
└── docs/
```

---

## Domain Model

### Entities

**User**
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| Username | string(50) | Unique |
| Email | string(255) | Unique |
| PasswordHash | string(255) | BCrypt |
| DisplayName | string(100) | |
| CreatedAt | DateTime | |

**HouseholdTask**
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| Title | string(200) | Required |
| Description | string | Optional |
| TaskType | enum | OneOff = 0, Recurring = 1 |
| Status | enum | Pending = 0, InProgress = 1, Completed = 2, Overdue = 3 |
| DueDate | DateTime? | |
| AssignedToUserId | Guid? | FK → Users |
| CreatedByUserId | Guid | FK → Users |
| TemplateId | Guid? | FK → RecurringTaskTemplates (if generated) |
| CreatedAt | DateTime | |
| CompletedAt | DateTime? | |

**RecurringTaskTemplate**
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| Title | string(200) | Required |
| Description | string | Optional |
| FrequencyDays | int | Minimum 1 |
| CurrentAssigneeIndex | int | Default 0 |
| LastGeneratedDate | DateTime? | |
| CreatedAt | DateTime | |

**RotationEntry**
| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| TemplateId | Guid | FK → RecurringTaskTemplates (CASCADE) |
| UserId | Guid | FK → Users |
| RotationOrder | int | Position in rotation |

### Rotation Logic

Each `RecurringTaskTemplate` has an ordered list of `RotationEntry` items. When a task is generated:
1. Assign to the user at `CurrentAssigneeIndex`
2. Advance index: `(CurrentAssigneeIndex + 1) % rotationEntries.Count`
3. Update `LastGeneratedDate`

---

## Database

### PostgreSQL Schema

```sql
CREATE TABLE IF NOT EXISTS migration_history (
    id SERIAL PRIMARY KEY,
    migration_name VARCHAR(255) UNIQUE NOT NULL,
    applied_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS recurring_task_templates (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title VARCHAR(200) NOT NULL,
    description TEXT,
    frequency_days INT NOT NULL,
    current_assignee_index INT NOT NULL DEFAULT 0,
    last_generated_date TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT NOW()
);

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

CREATE TABLE IF NOT EXISTS rotation_entries (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    template_id UUID NOT NULL REFERENCES recurring_task_templates(id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(id),
    rotation_order INT NOT NULL
);
```

### Migration Runner

- Located at `HomeFlow.Infrastructure/Database/MigrationRunner.cs`
- SQL files stored in `HomeFlow.Infrastructure/Database/Migrations/`
- Runs on app startup from `Program.cs`
- Creates `migration_history` table if not exists
- Reads `.sql` files ordered by filename prefix (001_, 002_, etc.)
- Skips migrations already recorded in `migration_history`
- Executes new migrations inside a transaction, records in history
- All SQL uses `IF NOT EXISTS` for extra idempotency safety

### Seeded Data

- 4 users: Pedro, Maria, João, Ana (password: "Password123!" for all, BCrypt-hashed)
- 2 recurring templates:
  - "Clean kitchen" — weekly (7 days), rotation: Pedro → Maria → João → Ana
  - "Take out trash" — every 3 days, rotation: Ana → João → Maria → Pedro
- 3 one-off tasks:
  - "Buy groceries" — assigned to Pedro, Pending, due tomorrow
  - "Fix bathroom faucet" — assigned to João, InProgress, due in 3 days
  - "Pay electricity bill" — assigned to Maria, Completed

---

## API Endpoints

### Auth (`/api/auth`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/register` | No | Create user |
| POST | `/api/auth/login` | No | Login, returns JWT |
| GET | `/api/auth/me` | Yes | Current user profile |

### Tasks (`/api/tasks`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/tasks` | Yes | List tasks (filter by assignee, status) |
| GET | `/api/tasks/{id}` | Yes | Get task by ID |
| POST | `/api/tasks` | Yes | Create one-off task |
| PUT | `/api/tasks/{id}` | Yes | Update task |
| DELETE | `/api/tasks/{id}` | Yes | Delete task |
| PATCH | `/api/tasks/{id}/complete` | Yes | Mark as completed |

### Recurring Templates (`/api/recurring-tasks`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/recurring-tasks` | Yes | List templates |
| GET | `/api/recurring-tasks/{id}` | Yes | Get template with rotation |
| POST | `/api/recurring-tasks` | Yes | Create template |
| PUT | `/api/recurring-tasks/{id}` | Yes | Update template |
| DELETE | `/api/recurring-tasks/{id}` | Yes | Delete template |
| POST | `/api/recurring-tasks/{id}/generate` | Yes | Generate next task in rotation |

### Health (`/api/health`)

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| GET | `/api/health` | No | Health check |

---

## Business Logic (Application Layer)

### UserService

- `Register(username, email, password, displayName)` — validate uniqueness, hash password (BCrypt), create user
- `Login(username, password)` — verify credentials, return JWT
- `GetById(userId)` — return user profile

**Validation:**
- Username: 3-50 characters
- Email: valid format
- Password: minimum 8 characters

### TaskService

- `CreateTask(title, description, dueDate, assignedToUserId, createdByUserId)` — create one-off task
- `GetAllTasks(filter?)` — list with optional filters (assignee, status)
- `GetTaskById(id)` — single task
- `UpdateTask(id, title, description, dueDate, assignedToUserId)` — update fields
- `CompleteTask(id, requestingUserId)` — mark completed, record timestamp
- `DeleteTask(id)` — remove task

**Validation:**
- Title: required, max 200 chars
- Due date: must be in future (for new tasks)
- Assigned user: must exist

### RecurringTaskService

- `CreateTemplate(title, description, frequencyDays, userIdsInOrder)` — create template + rotation entries
- `GetAllTemplates()` — list with rotation info
- `GetTemplateById(id)` — single template
- `UpdateTemplate(id, ...)` — update details or rotation order
- `DeleteTemplate(id)` — cascade delete
- `GenerateNextTask(templateId)` — create task for current assignee, advance rotation

**Validation:**
- Frequency: >= 1 day
- Rotation: at least one user

### Business Rules

- Completed tasks cannot be completed again
- Only assigned user or task creator can mark complete
- Rotation index wraps: `index % memberCount`
- Overdue detection: tasks past `dueDate` still Pending/InProgress are flagged Overdue at query time

---

## Frontend (home-flow-ui)

### Tech Stack

- React 18 + TypeScript + Vite
- Tailwind CSS (responsive layout)
- Framer Motion (animations, page transitions, gestures)
- Shadcn/ui (base components: dialogs, dropdowns, toasts)
- ESLint 9 + Prettier (code quality)

### Pages

- **Login** — username/password form, stores JWT in localStorage
- **Dashboard** — today's tasks, overdue count, task distribution per member
- **Tasks** — table/list with filters (assignee, status, type), create/edit/delete
- **Recurring Tasks** — templates list, create/edit with rotation order, generate button
- **Profile** — current user info

### Component Structure

```
home-flow-ui/src/
├── api/               # Fetch wrapper, auth interceptor
├── components/
│   ├── Layout/        # Navbar, Sidebar, ProtectedRoute
│   ├── Tasks/         # TaskList, TaskForm, TaskCard, StatusBadge
│   ├── Recurring/     # TemplateList, TemplateForm, RotationOrder
│   └── Dashboard/     # StatCard, TaskSummary, MemberDistribution
├── pages/             # LoginPage, DashboardPage, TasksPage, RecurringPage, ProfilePage
├── hooks/             # useAuth, useTasks, useRecurringTasks
├── types/             # TypeScript interfaces matching API DTOs
└── context/           # AuthContext (JWT state, current user)
```

### UX Approach

- Mobile-first responsive design (Tailwind breakpoints)
- **Mobile**: single-column, bottom nav, swipe-to-complete, full-screen modals
- **Desktop**: sidebar nav, multi-column dashboard, inline edits, hover states
- Framer Motion for: page transitions, card enter/exit animations, drag interactions, button micro-interactions
- Optimistic updates for task completion (instant feedback, rollback on error)
- Toast notifications for success/error feedback

### ESLint Configuration

- ESLint 9 flat config (`eslint.config.js`)
- Plugins: `@typescript-eslint`, `eslint-plugin-react-hooks`, `eslint-plugin-react-refresh`
- Rules: strict TS checks, no unused vars, no `any`, consistent imports, hooks rules
- Prettier integration via `eslint-config-prettier`
- Runs locally (`npm run lint`) and in CI (fails build on violations)

---

## Testing Strategy

### Backend (xUnit + NSubstitute + FluentAssertions)

**Domain Tests:**
- Entity validation
- Enum behavior
- Rotation index wrapping

**Application Tests:**
- `UserServiceTests` — registration, login, validation, duplicate detection
- `TaskServiceTests` — CRUD, permissions, overdue logic, completion rules
- `RecurringTaskServiceTests` — template CRUD, rotation advancement, task generation
- Mocked repositories via NSubstitute

**Infrastructure Tests:**
- Repository integration tests against real PostgreSQL (Testcontainers)
- MigrationRunner: idempotency, ordering, history tracking
- JWT generation and validation

**API Tests:**
- Integration tests with `WebApplicationFactory`
- Auth flow: register → login → protected endpoint
- 401 on unauthorized access
- Correct status codes and response shapes
- 400 on validation errors

### Frontend (Vitest + React Testing Library)

- Component tests: form validation, button actions, filter behavior
- Hook tests: `useAuth` login/logout, `useTasks` fetch/cache
- No E2E (out of scope, noted as future improvement)

---

## Development Environment

### Docker Compose

```yaml
services:
  db:
    image: postgres:16
    ports: ["5432:5432"]
    environment:
      POSTGRES_DB: homeflow
      POSTGRES_USER: homeflow
      POSTGRES_PASSWORD: homeflow_dev
    volumes:
      - pgdata:/var/lib/postgresql/data

  api:
    build: ./src
    ports: ["5000:8080"]
    depends_on: [db]
    environment:
      - ConnectionStrings__DefaultConnection=Host=db;Database=homeflow;Username=homeflow;Password=homeflow_dev

  ui:
    build: ./home-flow-ui
    ports: ["3000:3000"]
    depends_on: [api]

volumes:
  pgdata:
```

Run locally: `docker-compose up`

### GitHub Actions CI

```yaml
# .github/workflows/ci.yml
trigger: push/PR to main

jobs:
  backend-tests:
    - services: postgres:16
    - dotnet restore → build → test

  frontend-tests:
    - npm ci → lint → test

  build-images:
    - docker build API and UI (verify they build)
```

---

## Deployment (Railway)

- **API**: Railway web service, Dockerfile-based, auto-deploys from `main`
- **UI**: Railway web service (Vite preview build), auto-deploys from `main`
- **PostgreSQL**: Railway managed PostgreSQL plugin
- **Environment variables**: Connection string, JWT secret, CORS origins configured in Railway dashboard
- Railway handles deployment on merge to `main` — CI validates, Railway deploys

---

## Key Interfaces (Domain)

```csharp
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
}

public record TaskFilter(Guid? AssignedToUserId, TaskStatus? Status, TaskType? TaskType);

public interface ITaskRepository
{
    Task<HouseholdTask?> GetByIdAsync(Guid id);
    Task<IEnumerable<HouseholdTask>> GetAllAsync(TaskFilter? filter);
    Task<HouseholdTask> CreateAsync(HouseholdTask task);
    Task<HouseholdTask> UpdateAsync(HouseholdTask task);
    Task DeleteAsync(Guid id);
}

public interface IRecurringTaskTemplateRepository
{
    Task<RecurringTaskTemplate?> GetByIdAsync(Guid id);
    Task<IEnumerable<RecurringTaskTemplate>> GetAllAsync();
    Task<RecurringTaskTemplate> CreateAsync(RecurringTaskTemplate template);
    Task<RecurringTaskTemplate> UpdateAsync(RecurringTaskTemplate template);
    Task DeleteAsync(Guid id);
}

public interface IJwtTokenProvider
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
```
