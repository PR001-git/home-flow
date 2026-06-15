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

**Every Claude session must be documented.** At the end of each session, create a log file in `docs/sessions/` so that any engineer can read back exactly what was prompted, what Claude did, and why.

The goal is a clear, human-readable audit trail — useful for onboarding, code reviews, and understanding why the codebase looks the way it does.

### File naming

```
docs/sessions/YYYY-MM-DD-NNN-<short-slug>.md
```

- `YYYY-MM-DD` — date the session ran
- `NNN` — zero-padded sequence number for that day (001, 002, …)
- `<short-slug>` — 2–5 words describing what was worked on, kebab-cased

**Examples:**
```
docs/sessions/2026-06-14-001-backend-setup.md
docs/sessions/2026-06-14-002-auth-endpoints.md
docs/sessions/2026-06-15-001-chore-rotation-logic.md
```

### File template

```markdown
# <Title>

**Date:** YYYY-MM-DD
**Branch:** <branch>
**Session file:** docs/sessions/YYYY-MM-DD-NNN-<slug>.md

---

## Prompts & Interactions

Each prompt the engineer sent during this session, in order.

### 1. <Short label for this prompt>

**Prompt:**
> Paste the engineer's exact prompt here.

**What Claude did:**
- Brief bullet list of actions taken (files created/edited, decisions made, etc.)

**Outcome:**
One sentence on the result — what exists now that didn't before, or what changed.

---

### 2. <Short label>

**Prompt:**
> ...

**What Claude did:**
- ...

**Outcome:**
...

---

## Session summary

2–4 sentences covering: what was the goal of this session, what was built or decided, and anything left open or deferred.

```

### Rules

- **Log every prompt** — even short follow-ups and corrections. The sequence matters; it shows how the engineer guided Claude to the final result.
- **Verbatim prompts** — copy the engineer's words exactly, do not paraphrase.
- **Outcomes, not narration** — "What Claude did" is a bullet list of concrete actions, not a story.
- **One file per session** — a session is one continuous conversation. If you open a new chat, that is a new session file.
- **Create the file before closing the session** — do not reconstruct from memory later.
