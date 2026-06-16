# Reader-by-name mapping refactor

**Date:** 2026-06-16
**Branch:** refactor/reader-by-name-mapping
**Session file:** docs/sessions/2026-06-16-001-reader-by-name-mapping.md

---

## Prompts & Interactions

### 1. Why do only repositories have interfaces in Domain?

**Prompt:**
> why just  repositories has interfaces allocated in domain project?

**What Claude did:**
- Inspected `src/HomeFlow.Domain` (Entities, Enums, Repositories).
- Explained the Dependency Inversion Principle: repository interfaces are the only thing the Domain needs but is not allowed to implement (they cross the boundary to the database). Entities/enums are already pure, so they need no interface.

**Outcome:**
No code change — conceptual explanation of why repository interfaces live in Domain while implementations live in Infrastructure.

---

### 2. Why not FluentValidation instead of service-level validation?

**Prompt:**
> Another question. Why not create FluentValidation classes instead to handle validation in services?

**What Claude did:**
- Reviewed `TaskService` and `ValidationException`.
- Explained the trade-off: most validation here is behavioral/stateful (repository lookups, permission and status checks) and must live in the service anyway; the inline approach keeps dependencies minimal, consistent with the project's "raw Npgsql, no heavy frameworks" stance. Noted duplication between create/update as the main downside FluentValidation (or a shared helper) would address.

**Outcome:**
No code change — analysis of the validation approach and where FluentValidation would and would not help.

---

### 3. Avoid positional ordinal reads in repositories

**Prompt:**
> there is other better way to  get the data from the read to avoid [positional ordinal mapping] ... use the position, if a change the sql it will broken the rest of the code

**What Claude did:**
- Explained that reading by column name via `GetOrdinal` makes mapping resilient to SELECT column-order changes.
- Proposed a shared `DataReaderExtensions` + per-entity `Map` pattern and offered to roll it out.

**Outcome:**
No code change yet — agreed on the shared extension approach.

---

### 4. Roll out the shared pattern across all four repositories

**Prompt:**
> roll out the shared DataReaderExtensions + Map pattern across all four repositories.
> you should create a branch for these fix before commit

**What Claude did:**
- Created branch `refactor/reader-by-name-mapping`.
- Added `src/HomeFlow.Infrastructure/Database/DataReaderExtensions.cs` with `Get<T>`, `GetNullable<T>`, `GetNullableString`, and `GetEnum<TEnum>` (smallint → enum) name-based helpers.
- Refactored all four repositories to map columns by name:
  - `RotationEntryRepository` — extracted a `MapFromReader` method.
  - `UserRepository` — name-based reads in `ReadUserAsync`.
  - `TaskRepository` — name-based reads incl. nullable columns and enums in `MapFromReader`.
  - `RecurringTaskTemplateRepository` — name-based reads in `MapFromReader`.
- Built the Infrastructure project and full solution (clean), ran Application unit tests (12/12 pass).
- Could not run the Infrastructure/API integration tests: Docker Desktop is not running, so Testcontainers fails to start Postgres before reaching the changed code.

**Outcome:**
All four repositories now map by column name instead of ordinal position, backed by a shared `DataReaderExtensions` helper; verified by clean build and passing unit tests, with integration tests pending a Docker environment.

---

## Session summary

The session began with two architectural Q&A exchanges (why repository interfaces live in Domain, and why validation is done in services rather than via FluentValidation), then moved to a concrete refactor. To remove the fragility of positional `reader.GetXxx(ordinal)` mapping, a shared `DataReaderExtensions` helper was added and all four Infrastructure repositories were updated to read columns by name. The solution builds cleanly and Application unit tests pass; the repository integration tests could not be executed because Docker was unavailable in this environment, so they should be run before merge.
