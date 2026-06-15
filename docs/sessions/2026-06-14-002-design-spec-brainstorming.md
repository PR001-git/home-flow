# HomeFlow — Design Spec Brainstorming

**Date:** 2026-06-14
**Branch:** feat/backend
**Session file:** docs/sessions/2026-06-14-002-design-spec-brainstorming.md

---

## Prompts & Interactions

### 1. Initial brainstorming request

**Prompt:**
> I have 4 people in my household, we need split the tasks evenly. I need an app to organize the home routine and make sure tasks are completed on time.
>
> Also, I will use this idea to implement a tecnical challenge for a job interview. You should follow the guidelines @docs/Technical_Interview_Exercise.md for the technical challenge.
>
> I want to follow the SDD + TDD. I need document every step of the process.

**What Claude did:**
- Invoked `superpowers:brainstorming` skill
- Read `docs/Technical_Interview_Exercise.md` for interview requirements
- Explored project state (found it mostly empty — just settings.json and the exercise doc)
- Created task list to track brainstorming process
- Offered visual companion (browser-based mockups/diagrams)

**Outcome:**
Brainstorming session initiated with full context of the technical interview constraints (no EF/Dapper/Mediator, Clean Architecture, TDD, React frontend).

---

### 2. Visual companion declined

**Prompt:**
> no

**What Claude did:**
- Marked visual companion step complete
- Proceeded to clarifying questions in text-only mode

**Outcome:**
Session continues without browser-based visuals.

---

### 3. Task types question

**Prompt:**
> c

**What Claude did:**
- Recorded that the app needs both recurring chores (with rotation) and one-off tasks (with deadlines and manual assignment)

**Outcome:**
Domain scoped to support two task types: recurring with rotation + one-off with assignment.

---

### 4. Accountability model question

**Prompt:**
> c

**What Claude did:**
- Recorded the chosen model: rotation for recurring chores + manual assignment with due dates for one-off tasks

**Outcome:**
Accountability approach decided: automatic rotation for recurring, due-date tracking for one-off.

---

### 5. Database choice

**Prompt:**
> b

**What Claude did:**
- Selected PostgreSQL with raw Npgsql as the data storage solution

**Outcome:**
PostgreSQL chosen — demonstrates versatility beyond the Microsoft ecosystem.

---

### 6. Frontend framework choice

**Prompt:**
> A

**What Claude did:**
- Selected React with TypeScript as the frontend framework

**Outcome:**
React + TypeScript confirmed for the frontend.

---

### 7. Authentication approach

**Prompt:**
> A

**What Claude did:**
- Selected JWT tokens for stateless authentication

**Outcome:**
JWT-based auth chosen — industry standard for SPAs, good interview demonstration.

---

### 8. Household scope question

**Prompt:**
> A

**What Claude did:**
- Scoped the app to a single household with 4 seeded users for the demo

**Outcome:**
Single-household scope keeps the project focused for the interview.

---

### 9. Architecture approach selection

**Prompt:**
> A

**What Claude did:**
- Selected Approach A: Repository Pattern with raw ADO.NET/Npgsql
- Presented the recommendation as the most textbook Clean Architecture approach that demonstrates what EF abstracts away

**Outcome:**
Architecture decided: repository pattern with hand-written Npgsql, strict Clean Architecture layers.

---

### 10. Domain model approval

**Prompt:**
> yes

**What Claude did:**
- Presented Section 2: Clean Architecture layers and project structure
- Defined dependency rules and key interfaces

**Outcome:**
Domain model (User, HouseholdTask, RecurringTaskTemplate, RotationEntry) and rotation logic approved.

---

### 11. Rename frontend folder

**Prompt:**
> rename frontend to home-flow-ui

**What Claude did:**
- Updated the project structure to use `home-flow-ui/` instead of `frontend/`
- Presented Section 3: API Endpoints

**Outcome:**
Frontend directory renamed to `home-flow-ui` in the design.

---

### 12. API endpoints approval

**Prompt:**
> yes

**What Claude did:**
- Presented Section 4: Data Layer & Database Schema (PostgreSQL tables, data access approach, seeded data)

**Outcome:**
API surface approved (Auth, Tasks, Recurring Templates, Health endpoints).

---

### 13. Migration runner request

**Prompt:**
> yes but add a migrationrunner and a db folter to store the migrations make sure indepotency.

**What Claude did:**
- Added MigrationRunner to the design with:
  - `migration_history` table for tracking applied migrations
  - `Database/Migrations/` folder with numbered SQL files
  - Idempotency via history check + `IF NOT EXISTS` in SQL
  - Transaction-wrapped execution on app startup

**Outcome:**
Migration runner with idempotent SQL migrations added to the Infrastructure layer design.

---

### 14. Revised data layer approval

**Prompt:**
> yes

**What Claude did:**
- Presented Section 5: Business Logic Layer (UserService, TaskService, RecurringTaskService with validation rules and business rules)

**Outcome:**
Revised data layer with migration runner approved.

---

### 15. Business logic approval

**Prompt:**
> yes

**What Claude did:**
- Presented Section 6: Frontend design (pages, component structure, styling approach question)

**Outcome:**
Business logic layer approved (services, validation, business rules including rotation and permissions).

---

### 16. Modern UX request

**Prompt:**
> i want a modern UX experience, with animations etc.

**What Claude did:**
- Recommended Tailwind CSS + Framer Motion + Shadcn/ui for a production-quality modern UX
- Described animation capabilities: page transitions, card enter/exit, drag interactions, micro-interactions

**Outcome:**
Frontend styling stack decided: Tailwind + Framer Motion + Shadcn/ui for polished animations.

---

### 17. Responsive design question

**Prompt:**
> is it will be responsive? working in mobile and desktop web browser ?

**What Claude did:**
- Confirmed fully responsive design with mobile-first approach
- Detailed mobile vs desktop behavior: bottom nav, swipe-to-complete, single-column vs sidebar layout

**Outcome:**
Responsive design confirmed — mobile-first with progressive enhancement for desktop.

---

### 18. Responsive confirmation

**Prompt:**
> yes

**What Claude did:**
- Presented Section 7: Testing Strategy (xUnit, NSubstitute, FluentAssertions, Testcontainers, Vitest, React Testing Library)

**Outcome:**
Frontend design section fully approved.

---

### 19. Testing strategy approval

**Prompt:**
> yes

**What Claude did:**
- Asked about deployment and dev environment considerations

**Outcome:**
Testing strategy approved (backend: xUnit + NSubstitute + FluentAssertions + Testcontainers; frontend: Vitest + RTL).

---

### 20. Deployment and dev environment question

**Prompt:**
> what about the deployment and dev run envionment.
> Considerer use docker compose ? some github actions and free deploy?

**What Claude did:**
- Presented Section 8: Docker Compose for local dev, GitHub Actions CI pipeline
- Asked about free deployment platform preference (Render, Railway, Azure, or local-only)

**Outcome:**
Docker Compose and GitHub Actions CI added to the design; deployment platform question posed.

---

### 21. Railway deployment choice

**Prompt:**
> B

**What Claude did:**
- Selected Railway.app for deployment (API, UI, and managed PostgreSQL)
- Described the CI/CD flow: GitHub Actions validates, Railway auto-deploys on merge to main

**Outcome:**
Railway chosen for free deployment — auto-deploys from main branch.

---

### 22. ESLint request

**Prompt:**
> let's put some eslint as well? to verify clean code?

**What Claude did:**
- Added ESLint 9 with flat config, TypeScript plugin, React hooks plugin, Prettier integration
- Runs locally and in CI (fails build on violations)

**Outcome:**
ESLint + Prettier added to frontend tooling and CI pipeline.

---

### 23. ESLint confirmation

**Prompt:**
> yes

**What Claude did:**
- Wrote the full design spec to `docs/superpowers/specs/2026-06-14-home-flow-design.md`
- Ran spec self-review: found one ambiguity (TaskFilter type undefined), fixed it inline
- Asked user to review the written spec

**Outcome:**
Complete design specification written and self-reviewed; awaiting user review.

---

### 24. Session log request

**Prompt:**
> Read CLAUDE.md and follow the Session Logging convention exactly.
>
> Then review our entire conversation from the beginning and produce the session log file for this session.
>
> - File path: docs/sessions/YYYY-MM-DD-NNN-<slug>.md (use today's date, pick the next available sequence number, and derive the slug from what we worked on)
> - Copy every prompt I sent verbatim, in order
> - For each prompt, list what you did and the one-sentence outcome
> - End with a session summary and files touched
>
> Write the file directly — do not ask for confirmation.

**What Claude did:**
- Read CLAUDE.md for session logging conventions
- Checked `docs/sessions/` and found `001` already taken
- Wrote this session log as `002`

**Outcome:**
Session log created at `docs/sessions/2026-06-14-002-design-spec-brainstorming.md`.

---

## Session summary

This session completed the full brainstorming and design phase for HomeFlow. Through iterative clarifying questions, we defined: a single-household app with 4 seeded users, recurring chores with automatic rotation + one-off tasks with due dates, PostgreSQL with raw Npgsql (no ORM), JWT auth, React + TypeScript + Tailwind + Framer Motion frontend, Docker Compose local dev, GitHub Actions CI, and Railway deployment. The complete design spec was written, self-reviewed, and is awaiting user approval before moving to implementation planning.

## Files touched

- `docs/superpowers/specs/2026-06-14-home-flow-design.md` — created (full design specification)
- `docs/sessions/2026-06-14-002-design-spec-brainstorming.md` — created (this file)
