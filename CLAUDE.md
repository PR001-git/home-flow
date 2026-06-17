# HomeFlow — CLAUDE.md

## Conventions

### Architecture
- Strict Clean Architecture: Domain → Application → Infrastructure → API. No layer may depend on a layer above it.
- No Entity Framework, Dapper, or Mediator — raw Npgsql only for all database access.

### C# style
- Prefer primary constructors for all classes and records.

### Development workflow
- TDD is required. Write the failing test first, then the implementation.
- All new features must have unit and/or integration tests before being merged.

---

## Session Logging (Required)

**Every Claude session must be documented.** At the end of each session, create a log file in `docs/sessions/` named `YYYY-MM-DD-NNN-<short-slug>.md`.

Follow the naming convention, template, and rules in [docs/sessions/TEMPLATE.md](docs/sessions/TEMPLATE.md). Key rules: log every prompt verbatim and in order; outcomes as concrete bullets, not narration; one file per session; create it before closing the session.
