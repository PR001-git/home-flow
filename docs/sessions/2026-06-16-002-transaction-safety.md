# Add transaction safety for multi-write operations

**Date:** 2026-06-16
**Branch:** refactor/reader-by-name-mapping
**Session file:** docs/sessions/2026-06-16-002-transaction-safety.md

---

## Prompts & Interactions

### 1. Audit for missing transactions

**Prompt:**
> check service and repository looking for places with missed transaction  where it should be required to avoid messy data

**What Claude did:**
- Read all Application services (`RecurringTaskService`, `TaskService`, `UserService`) and all Infrastructure repositories.
- Identified that every repository method opens and closes its own `NpgsqlConnection`, and no transaction abstraction existed anywhere in the codebase.
- Flagged three multi-write service operations that could leave orphaned/inconsistent data if a later step failed:
  - `RecurringTaskService.CreateTemplateAsync` — template created, then rotation entries inserted in a loop; a failed insert leaves a template with partial/no rotation members.
  - `RecurringTaskService.UpdateTemplateAsync` — old rotation entries deleted before new ones are inserted; a failed insert mid-loop permanently loses all rotation members.
  - `RecurringTaskService.GenerateNextTaskAsync` — task created, then template's rotation index/last-generated-date updated; if the second write fails, the rotation pointer never advances, causing duplicate assignments.

**Outcome:**
Produced a written audit identifying the three at-risk methods and the root cause (no shared connection/transaction abstraction across repository calls).

---

### 2. Implement the fix

**Prompt:**
> yes keep going

**What Claude did:**
- Added `IUnitOfWork` (Domain layer, `src/HomeFlow.Domain/Repositories/IUnitOfWork.cs`) exposing `BeginTransactionAsync`/`CommitAsync`/`RollbackAsync`, kept free of Npgsql types to respect Clean Architecture layering.
- Added `UnitOfWork` (Infrastructure layer, `src/HomeFlow.Infrastructure/Database/UnitOfWork.cs`) — a scoped class that lazily opens and shares a single `NpgsqlConnection` plus an optional `NpgsqlTransaction` for the lifetime of the request.
- Changed `RecurringTaskTemplateRepository`, `RotationEntryRepository`, and `TaskRepository` to take the concrete `UnitOfWork` instead of `IDbConnectionFactory`, running every command against the shared connection/transaction instead of opening a new connection per call. `UserRepository` was left untouched since none of its operations participate in a multi-write flow.
- Updated `RecurringTaskService` to take an `IUnitOfWork` and wrapped the three at-risk methods (`CreateTemplateAsync`, `UpdateTemplateAsync`, `GenerateNextTaskAsync`) in begin/commit/rollback-on-exception blocks, keeping pre-transaction validation (existence checks) outside the transaction since those are read-only.
- Registered `UnitOfWork` as scoped in `Program.cs` and bound `IUnitOfWork` to the same scoped instance so service and repositories share one connection/transaction per request.
- Practiced TDD: added a failing test (`CreateTemplate_RotationEntryInsertFails_RollsBackAndDoesNotCommit`) and rollback/commit assertions to existing tests in `RecurringTaskServiceTests.cs` before implementing, confirmed it failed to compile against the old constructor, then implemented the production code until all tests passed.
- Verified `dotnet build` succeeds for the whole solution and all 13 `HomeFlow.Application.Tests` pass. Docker was not available in this environment, so the Testcontainers-backed `HomeFlow.Infrastructure.Tests` and `HomeFlow.API.Tests` could not be executed locally — only compiled successfully.

**Outcome:**
The three previously non-atomic multi-write flows in `RecurringTaskService` now run inside a database transaction and roll back cleanly on any failure, eliminating the risk of orphaned rotation entries or a stuck rotation index. `TaskService` and `UserService` were not changed since their writes are already single-statement and didn't need transaction wrapping.

---

## Session summary

Goal was to find and fix places where multiple related writes lacked transactional atomicity, risking inconsistent ("messy") data on partial failure. Found three such spots in `RecurringTaskService`, all stemming from the lack of any shared-connection/transaction abstraction across repositories. Implemented an `IUnitOfWork`/`UnitOfWork` pair (Domain abstraction + Infrastructure implementation using raw Npgsql, no EF/Dapper/Mediator) and wired the affected repositories and service through it, following TDD. Remaining open item: the username/email existence checks in `UserService.RegisterAsync` still have a check-then-insert race that a transaction alone won't close (would need a unique constraint or `SERIALIZABLE` isolation) — left out of scope since it's a different class of issue from the orphaned-write problem this session addressed.
