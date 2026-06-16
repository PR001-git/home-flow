# HomeFlow — Technical Documentation

> A maintenance-oriented guide to the whole system. If you are new to the codebase,
> read this top-to-bottom once, then keep it open as a map while you work.

For the original product/design rationale see
[specs/2026-06-14-home-flow-design.md](superpowers/specs/2026-06-14-home-flow-design.md).
This document describes **what is actually built and how to work on it**.

---

## Table of Contents

1. [Quick Start — Run It](#1-quick-start--run-it)  ← *do this first*
2. [What HomeFlow Is](#2-what-homeflow-is)
3. [Solution Layout](#3-solution-layout-the-10-second-map)
4. [Backend Architecture](#4-backend-architecture)
5. [API Reference](#5-api-reference)
6. [Frontend Architecture](#6-frontend-architecture)
7. [Local Development (running pieces natively)](#7-local-development-running-pieces-natively)
8. [Testing](#8-testing)
9. [CI/CD](#9-cicd)
10. [Conventions & Gotchas](#10-conventions--gotchas-read-before-your-first-pr)
11. [Where to Look When…](#11-where-to-look-when)

---

## 1. Quick Start — Run It

The fastest way to see HomeFlow working is the **one-command start scripts**, which build
the Docker images (including the UI image), bring up the whole stack, wait for it to be
healthy, and open the browser for you. **This is the first thing to do.**

### 1.1 The start scripts (recommended)

Two PowerShell helpers wrap `docker compose` so you don't have to remember flags or poll
for readiness:

| Script | What it does | When to use |
|--------|--------------|-------------|
| [`start.ps1`](../start.ps1) | Builds + starts the full stack, waits for the **API health check** (`/api/health`, up to 60s), opens the UI, then streams logs. `-Detach` leaves it running in the background instead of streaming logs. | Everyday "just run it", especially when you want to watch logs. |
| [`scripts/up.ps1`](../scripts/up.ps1) | Builds + starts detached, polls the **UI** at `http://localhost:3000` until it returns 200, then opens the browser. Runnable from anywhere (resolves the repo root itself). | Quick "bring it up and open it" with no log noise. |

```powershell
.\start.ps1            # attached: streams logs, Ctrl-C stops the stream
.\start.ps1 -Detach    # background: stack keeps running after the script exits
.\scripts\up.ps1       # detached + waits for the UI, then opens the browser
```

Both run `docker compose ... up --build`, so the **`--build` flag rebuilds the images every
time** — your latest code is always what runs.

### 1.2 What "building the UI image" actually means

The frontend is **not** served by Vite in this stack — it's built into static files and
served by **nginx**. The build is a two-stage Docker build defined in
[`home-flow-ui/Dockerfile`](../home-flow-ui/Dockerfile):

1. **Build stage** (`node:22-alpine`): `npm ci` then `npm run build` → produces the static
   bundle in `dist/`.
2. **Runtime stage** (`nginx:1.27-alpine`): copies `dist/` into nginx's web root and applies
   [`home-flow-ui/nginx.conf`](../home-flow-ui/nginx.conf).

That nginx config does two important things:
- **SPA fallback** — `try_files $uri $uri/ /index.html` so React Router's client-side routes (e.g. `/tasks`) resolve instead of 404-ing.
- **API reverse proxy** — requests to `/api/` are proxied to `http://api:8080` on the compose network. This is why the frontend can call `/api/...` with no CORS issues and no hardcoded API host: in the container the UI and API are same-origin via nginx.

The API image is built from the repo-root [`Dockerfile`](../Dockerfile). `docker-compose.yml`
wires the three services (`db`, `api`, `ui`) together; `--build` (used by both scripts)
triggers both image builds.

### 1.3 Plain docker compose (no script)

```bash
docker compose up -d --build      # db + api + ui, rebuild images
```
- UI → http://localhost:3000  ·  API → http://localhost:5000  ·  DB → localhost:5432
- The API waits for the DB healthcheck, then runs migrations + seed on startup.
- Log in with any seeded user, e.g. `pedro` / `Password123!`.
- Stop with `docker compose down` (add `-v` to wipe the database volume).

> If you only need to iterate on backend or frontend code in isolation (hot reload, native
> debugging), use the native workflows in [§7](#7-local-development-running-pieces-natively)
> instead of the container build.

---

## 2. What HomeFlow Is

A household task manager for a small, fixed household (4 members). It does three things:

1. **Recurring chores** — templates that rotate automatically through members (kitchen cleaning every 7 days: Pedro → Maria → João → Ana → …).
2. **One-off tasks** — manually assigned, with a due date ("buy groceries by Friday").
3. **Accountability** — status tracking, automatic overdue detection, and a dashboard showing how work is distributed across members.

The codebase is two deployable apps plus a database:

| Piece | Tech | Location | Port (local) |
|-------|------|----------|--------------|
| Backend API | .NET 8, ASP.NET Web API | `src/` | 5000 → 8080 |
| Frontend SPA | React 18 + TypeScript + Vite | `home-flow-ui/` | 3000 |
| Database | PostgreSQL 16 | `docker-compose.yml` | 5432 |

---

## 3. Solution Layout (the 10-second map)

```
home-flow/
├── src/                          # .NET backend (Clean Architecture, 4 projects)
│   ├── HomeFlow.Domain/          # entities, enums, repository INTERFACES — zero dependencies
│   ├── HomeFlow.Application/     # services (business logic), DTOs, validation
│   ├── HomeFlow.Infrastructure/  # Npgsql repos, JWT, migrations — IMPLEMENTS Domain interfaces
│   └── HomeFlow.API/             # controllers, middleware, DI wiring (Program.cs)
├── home-flow-ui/                 # React SPA
│   ├── Dockerfile                # 2-stage: npm build → nginx static serve (the UI image)
│   └── nginx.conf                # SPA fallback + /api reverse proxy to the api service
├── tests/                        # one xUnit project per backend layer
├── docs/                         # specs, plans, session logs, THIS file
├── start.ps1                     # build + run full stack, wait for API health, open browser
├── scripts/up.ps1                # build + run, wait for UI, open browser (run from anywhere)
├── docker-compose.yml            # db + api + ui for local/integration
├── Dockerfile                    # builds the API image
└── .github/workflows/ci.yml      # backend + frontend pipelines
```

---

## 4. Backend Architecture

### 4.1 The dependency rule (this is the spine of the codebase)

```
        API ─────────► Application ─────────► Domain ◄───────── Infrastructure
     (controllers)      (services,            (entities,        (Npgsql repos,
                         DTOs, rules)          interfaces)        JWT, migrations)
```

**The rule:** dependencies only ever point *inward* toward Domain. Domain depends on
nothing. Infrastructure depends on Domain (it implements Domain's interfaces). API
depends on Application and — only for DI wiring in `Program.cs` — Infrastructure.

**Why it matters for maintenance:** business logic in Application never knows about
Npgsql, HTTP, or JWT. You can change the database access code without touching a single
service, because services talk to `IUserRepository`, not `UserRepository`. When adding
code, ask "which layer owns this?" before writing it:

- A new business rule → **Application** (a service).
- A new database query → **Infrastructure** (a repository), declared as an interface in **Domain**.
- A new field on a thing → **Domain** entity + migration + repository mapping.
- A new endpoint → **API** controller, delegating to an Application service.

> **Hard constraints (from CLAUDE.md):** No EF Core, no Dapper, no MediatR.
> All DB access is **raw Npgsql**. All classes/records use **primary constructors**.

### 4.2 Layer-by-layer

**Domain** (`HomeFlow.Domain/`)
- `Entities/` — `User`, `HouseholdTask`, `RecurringTaskTemplate`, `RotationEntry`.
- `Enums/` — `HouseholdTaskStatus` (Pending/InProgress/Completed/Overdue), `HouseholdTaskType` (OneOff/Recurring).
- `Repositories/` — the interfaces (`IUserRepository`, `ITaskRepository`, `IRecurringTaskTemplateRepository`, `IRotationEntryRepository`, `IUnitOfWork`). These are *contracts*; the implementations live in Infrastructure.

**Application** (`HomeFlow.Application/`)
- `Services/` — `UserService`, `TaskService`, `RecurringTaskService`, `DashboardService`. All business logic lives here.
- `DTOs/` — request/response shapes grouped by feature (`Auth/`, `Tasks/`, `RecurringTasks/`, `Users/`, `Dashboard/`). Controllers and services speak DTOs, never entities, at the boundary.
- `Exceptions/` — `NotFoundException`, `ValidationException`. Thrown by services, translated to HTTP status codes by middleware (see §4.4).
- `Interfaces/IJwtTokenProvider.cs` — the contract for token issuing/validation, implemented in Infrastructure.

**Infrastructure** (`HomeFlow.Infrastructure/`)
- `Repositories/` — the Npgsql implementations of the Domain interfaces.
- `Database/` — connection factory, `UnitOfWork`, `MigrationRunner`, `DataReaderExtensions` (typed column reads), and the SQL `Migrations/`.
- `Auth/JwtTokenProvider.cs` — HMAC-signed JWT issuing and validation.

**API** (`HomeFlow.API/`)
- `Controllers/` — thin HTTP adapters; they validate input shape, call a service, return a DTO.
- `Middleware/ExceptionHandlingMiddleware.cs` — central error → HTTP mapping.
- `Infrastructure/GlobalRoutePrefixConvention.cs` — prepends `api` to every route so controllers don't repeat it.
- `Program.cs` — composition root: reads config, registers everything in DI, runs migrations on startup, wires the middleware pipeline.

### 4.3 Request lifecycle (follow one request end-to-end)

`POST /api/tasks` with a JWT:

1. **Kestrel → middleware pipeline** (`Program.cs:79-83`): `ExceptionHandlingMiddleware` → CORS → Authentication → Authorization → controller routing.
2. **`TasksController`** deserializes the `CreateTaskRequest` DTO, pulls the caller's user id from JWT claims, calls `TaskService.CreateTaskAsync(...)`.
3. **`TaskService`** validates (title required, due date in the future, assignee exists), constructs a `HouseholdTask` entity, calls `ITaskRepository.CreateAsync`.
4. **`TaskRepository`** runs a raw Npgsql `INSERT ... RETURNING`, maps the row back to an entity.
5. Service maps entity → `TaskResponse` DTO, controller returns `201 Created`.

If step 3 throws `ValidationException`, the middleware turns it into `400`; `NotFoundException` → `404`. Controllers stay clean of try/catch.

### 4.4 Cross-cutting mechanisms

**Dependency Injection** — all wiring is in `Program.cs`. Lifetimes:
- `Singleton`: `IDbConnectionFactory`, `MigrationRunner` (stateless, app-lifetime).
- `Scoped`: `UnitOfWork`, all repositories, all services, `IJwtTokenProvider` — one instance per HTTP request, so a request shares one DB connection/transaction.

**Unit of Work / transactions** (`UnitOfWork.cs`) — owns a single lazily-opened
`NpgsqlConnection` per request. Repositories within a request share that connection, and
multi-step writes (e.g. "create template + its rotation entries") run inside one
transaction via `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`. This is the
hand-rolled replacement for what EF's `DbContext` would normally give you.

**Error handling** — `ExceptionHandlingMiddleware` is the single place that converts
exceptions to `ErrorResponse` JSON + status code. Add a new mapping here, not in
controllers.

**Routing** — `GlobalRoutePrefixConvention("api")` means a controller annotated
`[Route("tasks")]` actually serves `/api/tasks`. Don't hardcode `api/` in route attributes.

**Auth** — JWT Bearer. `Program.cs:42-55` configures validation (issuer, audience,
lifetime, signing key). Protected endpoints use `[Authorize]`; `/api/auth/*` and
`/api/health` are open.

### 4.5 Database & migrations

- **No ORM.** Schema is defined by numbered SQL files in `Infrastructure/Database/Migrations/` (`001_…` → `005_SeedData.sql`).
- **`MigrationRunner`** runs on **every app startup** (`Program.cs:73-77`). It creates a `migration_history` table, reads `.sql` files in filename order, skips any already recorded, and runs new ones inside a transaction — then records them. Files also use `IF NOT EXISTS` for belt-and-suspenders idempotency.
- **To change the schema:** add a *new* numbered migration file. **Never edit an applied migration** — it won't re-run, and environments will drift.
- **Seed data** (`005_SeedData.sql`): 4 users (Pedro, Maria, João, Ana; password `Password123!`, BCrypt-hashed), 2 recurring templates, 3 one-off tasks. This is what the demo logs into.

Schema (4 tables + history): `users`, `recurring_task_templates`, `household_tasks`, `rotation_entries`. See the spec for the full DDL; it is reproduced verbatim in the migration files, which are the source of truth.

### 4.6 The rotation algorithm (the one piece of real domain logic)

Each `RecurringTaskTemplate` has an ordered list of `RotationEntry` rows (one per member,
with a `RotationOrder`). When `GenerateNextTask(templateId)` runs:

1. Create a `HouseholdTask` assigned to the member at `CurrentAssigneeIndex`.
2. Advance: `CurrentAssigneeIndex = (CurrentAssigneeIndex + 1) % rotationEntries.Count` — wraps around the household.
3. Set `LastGeneratedDate = now`.

**Overdue detection** is *computed at read time*, not stored: any task past its `DueDate`
that is still Pending/InProgress is surfaced as `Overdue`. So overdue status is always
fresh without a background job.

**Completion rules:** a Completed task can't be completed again; only the assignee or the
task's creator may complete it.

---

## 5. API Reference

All routes are prefixed `/api`. All return JSON. `ErrorResponse` shape on failure.

| Group | Method | Path | Auth | Purpose |
|-------|--------|------|------|---------|
| Auth | POST | `/auth/register` | — | Create user |
| Auth | POST | `/auth/login` | — | Login → JWT |
| Auth | GET | `/auth/me` | ✓ | Current user |
| Tasks | GET | `/tasks` | ✓ | List (filter: assignee, status, type) |
| Tasks | GET | `/tasks/{id}` | ✓ | One task |
| Tasks | POST | `/tasks` | ✓ | Create one-off |
| Tasks | PUT | `/tasks/{id}` | ✓ | Update |
| Tasks | DELETE | `/tasks/{id}` | ✓ | Delete |
| Tasks | PATCH | `/tasks/{id}/complete` | ✓ | Mark complete |
| Recurring | GET | `/recurring-tasks` | ✓ | List templates |
| Recurring | GET | `/recurring-tasks/{id}` | ✓ | Template + rotation |
| Recurring | POST | `/recurring-tasks` | ✓ | Create template |
| Recurring | PUT | `/recurring-tasks/{id}` | ✓ | Update template/rotation |
| Recurring | DELETE | `/recurring-tasks/{id}` | ✓ | Delete (cascades rotation) |
| Recurring | POST | `/recurring-tasks/{id}/generate` | ✓ | Generate next rotated task |
| Dashboard | GET | `/dashboard` | ✓ | Stats + distribution |
| Health | GET | `/health` | — | Liveness check |

A ready-to-import [Postman collection](HomeFlow.postman_collection.json) covers these.

---

## 6. Frontend Architecture

### 6.1 Stack & structure

React 18 + TypeScript, built with **Vite**, styled with **Tailwind CSS**, animated with
**Framer Motion**, data-fetching via **TanStack Query** hooks, routing via
**react-router-dom**. Tests: **Vitest + React Testing Library**.

```
home-flow-ui/src/
├── api/client.ts          # thin fetch wrapper (auth header, 401 event, error mapping)
├── types/index.ts         # TS interfaces mirroring backend DTOs
├── context/AuthContext.tsx# JWT + current-user state
├── hooks/                 # useAuth, useTasks, useRecurringTasks, useDashboard, useUsers
├── components/
│   ├── Layout/            # AppLayout, ProtectedRoute, PageTransition
│   ├── Tasks/             # TaskCard, TaskForm, StatusBadge
│   ├── Recurring/         # TemplateForm, RotationOrder
│   └── Dashboard/         # StatCard, MemberDistribution
├── pages/                 # Login, Dashboard, Tasks, Recurring, Profile
└── App.tsx                # route table
```

### 6.2 How the pieces connect

**Routing** (`App.tsx`): `/login` is public. Everything else is nested under
`<ProtectedRoute>` (redirects to login if no token) → `<AppLayout>` (nav chrome) →
the page. So adding an authenticated page = add one `<Route>` inside that nesting.

**API client** (`api/client.ts`): one `request<T>()` helper does all fetching. It
attaches `Authorization: Bearer <token>` from `localStorage` (`homeflow_token`), throws a
typed `ApiError` on non-2xx, and — importantly — on a `401` it dispatches a global
`homeflow:unauthorized` event. `AuthContext` listens for that and logs the user out, so an
expired token anywhere cleanly bounces to login. Exposed as `apiClient.get/post/put/patch/del`.

**Auth** (`context/AuthContext.tsx` + `hooks/useAuth.ts`): holds the token and current
user, persists the token to `localStorage`, exposes `login`/`logout`. Components consume
it through `useAuth()`.

**Server state** (the `hooks/` folder): each feature has a TanStack Query hook
(`useTasks`, `useRecurringTasks`, `useDashboard`, `useUsers`) that wraps `apiClient` calls
in queries/mutations. **This is where caching, refetching, and optimistic updates live** —
components don't call `apiClient` directly, they use these hooks. Task completion uses an
optimistic update (instant UI, rollback on error).

**Pages vs components**: pages compose hooks + components into a screen; components are
presentational/reusable. Keep data-fetching in hooks, not buried in components.

### 6.3 Adding a frontend feature (the typical path)

1. Add/confirm the DTO type in `types/index.ts` (must match the backend DTO).
2. Add a query/mutation in the relevant `hooks/` file calling `apiClient`.
3. Build presentational `components/`.
4. Compose them in a `pages/` screen; register a route in `App.tsx` if it's a new screen.
5. Add a `.test.tsx` alongside (the repo colocates tests).

---

## 7. Local Development (running pieces natively)

> To just run the whole app, use the start scripts in [§1](#1-quick-start--run-it).
> This section is for iterating on **one piece** with hot reload / native debugging,
> where you don't want to rebuild the Docker image on every change.

### 7.1 Backend natively

```bash
dotnet run --project src/HomeFlow.API     # needs a Postgres reachable via the connection string
```
Config comes from `appsettings.json` and environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpirationMinutes`). Start just the database with `docker compose up -d db` and point the connection string at `localhost:5432`.

### 7.2 Frontend natively

```bash
cd home-flow-ui
npm install
npm run dev      # Vite dev server with hot reload (see vite.config.ts for the /api proxy)
npm run build    # production build → dist/ (same step the UI Docker image runs)
npm run lint     # ESLint 9 flat config — CI fails on violations
npm test         # Vitest
```

In the dev server, Vite proxies `/api` to the backend (configured in `vite.config.ts`),
mirroring what nginx does in the built UI image (see [§1.2](#12-what-building-the-ui-image-actually-means)).
So API calls look identical (`/api/...`) whether you're in `npm run dev` or the container.

### 7.3 Configuration knobs

| Setting | Where | Notes |
|---------|-------|-------|
| DB connection | `ConnectionStrings__DefaultConnection` | env wins over appsettings |
| JWT signing key | `Jwt__Key` | **must be ≥ 32 bytes**; change in prod |
| JWT issuer/audience/expiry | `Jwt__*` | |
| CORS origin | `Program.cs:61-69` | currently hardcoded to `http://localhost:3000` |
| Postgres password | `POSTGRES_PASSWORD` | docker-compose default `homeflow_dev` |

---

## 8. Testing

**TDD is required** (CLAUDE.md): write the failing test first, then the implementation.
Every backend layer has a matching test project under `tests/`:

| Project | Style | What it covers |
|---------|-------|----------------|
| `HomeFlow.Application.Tests` | xUnit + NSubstitute + FluentAssertions | Service logic with **mocked repositories** — validation, rotation advancement, completion rules, overdue logic |
| `HomeFlow.Infrastructure.Tests` | xUnit + real Postgres | Repository queries, `MigrationRunner` idempotency/ordering, JWT issue/validate |
| `HomeFlow.API.Tests` | `WebApplicationFactory` integration | Full HTTP flow: register → login → protected call, 401/400/status-code correctness |
| `home-flow-ui` (`*.test.tsx`) | Vitest + RTL | Hooks (`useTasks`, `useAuth`), forms, pages |

Run them:
```bash
dotnet test                         # all backend test projects
cd home-flow-ui && npm test         # frontend
```

**Where to put a new test:** unit-test business rules against mocked repos in
`Application.Tests`; if it touches SQL, integration-test it in `Infrastructure.Tests`;
if it's an end-to-end HTTP contract, `API.Tests`.

---

## 9. CI/CD

**CI** (`.github/workflows/ci.yml`) runs on push/PR:
- **backend** — spins up a Postgres service, `dotnet restore → build → test`.
- **frontend** — `npm ci → lint → test → build`.
- **image build** — verifies the API and UI Docker images build.

A green pipeline is the merge gate. The frontend lint job will fail the build on any
ESLint violation, so run `npm run lint` before pushing.

---

## 10. Conventions & Gotchas (read before your first PR)

- **Respect the dependency rule.** If you find yourself adding a `using HomeFlow.Infrastructure` to a service, stop — you want a Domain interface instead.
- **Raw Npgsql only.** No EF/Dapper/MediatR, ever. Map rows by hand (`DataReaderExtensions` helps).
- **Primary constructors** for all C# classes/records.
- **Never edit an applied migration** — add a new numbered file.
- **Overdue is computed, not stored** — don't add an overdue-writing job.
- **Route prefix is automatic** — don't put `api/` in `[Route]`.
- **Frontend talks to the API only through `apiClient` + hooks** — don't scatter `fetch` calls.
- **DTO types must stay in sync** across `Application/DTOs/*` (C#) and `types/index.ts` (TS).
- **Session logging is mandatory.** Every Claude working session gets a log in `docs/sessions/` per [TEMPLATE.md](sessions/TEMPLATE.md): every prompt verbatim and in order, outcomes as concrete bullets, one file per session.

---

## 11. Where to Look When…

| You want to… | Go to |
|--------------|-------|
| Run the whole app now | [`start.ps1`](../start.ps1) or [`scripts/up.ps1`](../scripts/up.ps1) — see [§1](#1-quick-start--run-it) |
| Change how the UI image is built/served | [`home-flow-ui/Dockerfile`](../home-flow-ui/Dockerfile) + [`nginx.conf`](../home-flow-ui/nginx.conf) |
| Add an endpoint | `API/Controllers/` → delegate to an Application service |
| Change a business rule | `Application/Services/` |
| Add a DB query | `Infrastructure/Repositories/` (+ Domain interface) |
| Change the schema | new file in `Infrastructure/Database/Migrations/` |
| Change error→HTTP mapping | `API/Middleware/ExceptionHandlingMiddleware.cs` |
| Change DI / startup / pipeline | `API/Program.cs` |
| Add a UI screen | `home-flow-ui/src/pages/` + route in `App.tsx` |
| Change data fetching/caching | `home-flow-ui/src/hooks/` |
| Adjust auth behavior | `context/AuthContext.tsx` (FE), `Program.cs` + `Auth/JwtTokenProvider.cs` (BE) |
| Understand original intent | `docs/superpowers/specs/` |
| See what changed in a session | `docs/sessions/` |
```
