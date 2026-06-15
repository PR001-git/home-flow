# Code Review and Bug Fixes

**Date:** 2026-06-15
**Branch:** feat/backend
**Session file:** docs/sessions/2026-06-15-001-code-review-and-bug-fixes.md

---

## Prompts & Interactions

### 1. Code review and docker-compose build fix

**Prompt:**
> Do a code review and make sure everything is correct agains the specs and plans. I havo some issues with the docker compose build as well.

**What Claude did:**
- Ran `/code-review high` to spawn 3 parallel finder angles + verification pass across the full branch diff
- Identified 10 confirmed/plausible findings ranked by severity
- Root-caused the docker-compose build failure: no `.dockerignore` existed, so local `bin/` and `obj/` directories were sent in the Docker build context and copied via `COPY src/ src/`, corrupting the publish step
- Applied all fixes:
  1. **Created `.dockerignore`** — excludes `.git`, `.claude`, `**/bin/`, `**/obj/`, `docs/`, `.env` from the build context; resolves the docker-compose build failure
  2. **Fixed CI branch targeting** (`.github/workflows/ci.yml`) — changed `branches: [master]` to `[master, feat/backend]` so the pipeline actually runs during development
  3. **Added `needs: [backend-tests]` to `build-image` CI job** — prevents a broken Docker image from being built when tests fail
  4. **Fixed Overdue filter** (`TaskRepository.GetAllAsync`) — when filtering by `status=Overdue(3)`, now generates `AND status IN (0, 1) AND due_date < NOW()` instead of `AND status = 3` (which always returned 0 rows since Overdue is never written to the DB)
  5. **Fixed empty rotation crash** (`RecurringTaskService.GenerateNextTaskAsync` line 148) — added a guard that throws `ValidationException` if `entries.Count == 0`, and changed the index access to `entries[template.CurrentAssigneeIndex % entries.Count]` to handle stale index
  6. **Fixed missing due-date validation in `UpdateTaskAsync`** (`TaskService.cs`) — added `if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow) throw ValidationException(...)`, matching the check already present in `CreateTaskAsync`
  7. **Fixed case-sensitive username lookup** (`UserRepository.GetByUsernameAsync`) — changed `WHERE username = @username` to `WHERE LOWER(username) = LOWER(@username)`
  8. **Secured JWT key in `docker-compose.yml`** — changed hardcoded key to `${JWT_KEY:-docker-dev-secret-key-change-in-production-min-32-bytes}` so it can be overridden via a `.env` file without changing tracked files

**Outcome:**
All 8 fixes applied; `dotnet build` passes with 0 errors. The docker-compose build issue is resolved by the `.dockerignore`. `GET /tasks?status=3` now returns overdue tasks correctly. The CI pipeline will now run on every push to `feat/backend`.

---

## Session Summary

The session performed a full high-recall code review of the `feat/backend` branch against the spec and interview requirements, then applied all confirmed bugs as targeted fixes. The docker-compose build issue was caused by missing `.dockerignore` — local `bin/obj` artifacts contaminated the Docker build context. Eight bugs were fixed: the `.dockerignore`, CI branch mismatch, CI gate ordering, the Overdue query filter (a whole endpoint feature that was silently broken), an unguarded array crash in recurring task generation, missing due-date validation on task update, case-insensitive username login, and a hardcoded JWT secret. Three issues from the review were deferred (FK cascade on `template_id`, unique constraint on `rotation_entries`, and the deprecated `PostgreSqlBuilder` constructor warnings in tests) as they are lower priority and require schema migrations.
