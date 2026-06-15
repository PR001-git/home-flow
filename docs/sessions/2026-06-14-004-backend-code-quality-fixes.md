# Backend Code Quality Fixes

**Date:** 2026-06-14
**Branch:** feat/backend
**Session file:** docs/sessions/2026-06-14-004-backend-code-quality-fixes.md

---

## Prompts & Interactions

### 1. Code quality review and fixes

**Prompt:**
> a few things wrong in the code in the current generated code. Here are some of the issues:
>
> current state:
>
> - repositories: it had a connection string pass though the constructor, it should pass something more abstract, like a DbConnection or something, to avoid coupling the repository to a specific database technology. This would also make it easier to test the repositories by passing in a mock or in-memory connection. the same in MigrationRunner.
>
> - rename application layer to core layer but give me cons and pros of that. if pros wins then do it and make sure to change the docs files to reflect this change.
>
> - the opened PR says " Tasks are being implemented and committed one by one — watch commits land here." but there are no commits in per task made. you should make this happens. take care to commit each task separately, with a clear commit message describing what was done in that task. This will make it easier to review the code and understand the changes made for each task.
>
> - verify the settings to make sure that no sensitive information is exposed. If exposed, remove it and add it to .gitignore.
>
> - middlewares: should return a typed response instead of just a string.
>
> - /api should be removed from the controllers' route attributes, and instead be added as a global prefix in the startup configuration.
>
> - use primary constructors (introduce it in the claude.md but make sure that will be just fews lines).

**What Claude did:**
- Presented pros/cons for renaming Application → Core and recommended keeping "Application" (cons outweigh pros: "Application" is canonical Clean Architecture terminology; "Core" is ambiguous)
- Created `IDbConnectionFactory` interface and `NpgsqlConnectionFactory` implementation in `HomeFlow.Infrastructure/Database/`
- Updated all four repositories (`UserRepository`, `TaskRepository`, `RecurringTaskTemplateRepository`, `RotationEntryRepository`) and `MigrationRunner` to accept `IDbConnectionFactory` instead of a raw connection string
- Updated `Program.cs` to register `NpgsqlConnectionFactory` as a singleton and wire it into repositories
- Updated test fixtures (`DatabaseFixture`, `MigrationRunnerTests`, `UserRepositoryTests`) to use `NpgsqlConnectionFactory`
- Moved DB connection string and JWT key out of `appsettings.json` (empty placeholders remain) into `appsettings.Development.json`; removed `!appsettings.Development.json` exception from `.gitignore` so dev secrets are never committed
- Created `ErrorResponse` record in `HomeFlow.API/Models/` and updated `ExceptionHandlingMiddleware` to use it instead of anonymous types
- Created `GlobalRoutePrefixConvention` (implements `IApplicationModelConvention`) in `HomeFlow.API/Infrastructure/` and registered it in `AddControllers()`; removed `api/` prefix from all four controller `[Route]` attributes
- Applied C# 12 primary constructors to all services (`UserService`, `TaskService`, `RecurringTaskService`), `JwtTokenProvider`, `ExceptionHandlingMiddleware`, and all controllers
- Added `### C# style — Prefer primary constructors` convention to `CLAUDE.md`
- Each change was committed separately with a descriptive message

**Outcome:**
Six focused commits landed on `feat/backend`: IDbConnectionFactory abstraction, secrets gitignored, typed error responses, global API prefix, primary constructors, and test/build fixes.

---

## Session summary

The goal was a batch of code quality corrections to the generated .NET backend: decoupling repositories from Npgsql's constructor, securing committed config, adding type safety to error responses, centralising the route prefix, and adopting C# 12 primary constructors. All changes were implemented and committed individually. The Application layer rename was evaluated and rejected — "Application" remains as the canonical Clean Architecture name. No features were added; only structural and convention improvements.
