# HomeFlow Frontend — Design Specification

**Date:** 2026-06-16
**Branch:** feat/frontend
**Depends on:** `docs/superpowers/specs/2026-06-14-home-flow-design.md` (overall design)

## Goal

Build the HomeFlow web frontend (`home-flow-ui`) at full spec fidelity, plus two
small supporting read-only backend endpoints it requires. The result is a working
React + TypeScript SPA that authenticates against the existing API and exercises
every backend feature: one-off tasks, recurring templates with rotation, and a
dashboard.

## Scope

- **In scope:** React frontend (all 5 pages), two new backend endpoints
  (`GET /api/users`, `GET /api/dashboard`), frontend tests, CI extension.
- **Out of scope:** Changes to existing task/auth/recurring endpoints, deployment
  changes beyond CI (Railway wiring is a separate concern), E2E tests.

---

## 1. Backend Additions (TDD — build first)

Two read-only endpoints the UI depends on. Each follows existing patterns
(controller → application service → repository) with Application unit tests and an
API integration test.

### 1.1 `GET /api/users`

Returns household members for assignee dropdowns (TaskForm), the rotation builder
(RotationOrder), and distribution labels.

- Response: `[{ id: Guid, username: string, displayName: string }]`
- New: `UsersController`, `UserService.GetAllUsersAsync`, `IUserRepository.GetAllAsync`.
- Auth: required (Bearer JWT).
- Never returns `passwordHash` or `email` in this list DTO (`UserSummaryDto`).

### 1.2 `GET /api/dashboard`

Returns aggregates computed from existing repositories.

- Response:
  ```jsonc
  {
    "todaysTasks": [ /* HouseholdTask DTO */ ],
    "overdueCount": 3,
    "totalsByStatus": { "pending": 5, "inProgress": 2, "completed": 8, "overdue": 3 },
    "distribution": [ { "userId": "...", "displayName": "Pedro", "activeCount": 4 } ]
  }
  ```
- "Today's tasks" = tasks with `dueDate` on the current date (server date).
- "Active" in distribution = tasks not in `Completed` status, grouped by assignee.
- Overdue uses the **same query-time rule** already in `TaskService` (past `dueDate`
  and still Pending/InProgress) — no duplicated logic; reuse the existing helper.
- New: `DashboardController`, `DashboardService`, `DashboardDto`. No new tables.
- Auth: required.

---

## 2. Frontend Stack & Structure

- **Build:** Vite + React 18 + TypeScript.
- **Styling:** Tailwind CSS + Shadcn/ui (dialogs, dropdowns, toasts, buttons).
- **Motion:** Framer Motion (transitions, card enter/exit, drag, micro-interactions).
- **Server state:** TanStack Query (React Query) — caching, background refetch,
  optimistic updates with rollback.
- **Routing:** React Router v6.
- **Quality:** ESLint 9 flat config + Prettier, Vitest + React Testing Library.

```
home-flow-ui/src/
├── api/                # client.ts (typed fetch + auth), endpoint modules
├── components/
│   ├── Layout/         # Navbar, Sidebar, BottomNav, ProtectedRoute
│   ├── Tasks/          # TaskList, TaskForm, TaskCard, StatusBadge
│   ├── Recurring/      # TemplateList, TemplateForm, RotationOrder
│   └── Dashboard/      # StatCard, TaskSummary, MemberDistribution
├── pages/              # LoginPage, DashboardPage, TasksPage, RecurringPage, ProfilePage
├── hooks/              # useAuth, useTasks, useRecurringTasks, useUsers, useDashboard
├── types/              # TS interfaces matching API DTOs
└── context/            # AuthContext (JWT state, current user)
```

---

## 3. Data Flow & Auth

- **`api/client.ts`** — typed fetch wrapper. Injects `Authorization: Bearer <jwt>`
  from localStorage. On `401`, clears the token and redirects to Login. Throws
  typed errors for non-2xx so mutations can surface messages.
- **`AuthContext`** — holds the JWT and current user. On load with a stored token,
  bootstraps the user via `GET /api/auth/me`. Exposes `login`, `logout`, `user`.
- **`ProtectedRoute`** — redirects unauthenticated users to `/login`.
- **TanStack Query hooks** — `useTasks`, `useRecurringTasks`, `useUsers`,
  `useDashboard`. Mutations (create/update/delete/complete task, create/update/
  delete/generate template) use `onMutate` to apply an **optimistic update**, roll
  back on `onError`, and `invalidateQueries` on settle. Task completion is the
  flagship optimistic interaction (instant feedback, rollback on failure).

---

## 4. Pages (full-spec fidelity)

- **Login** — username/password form; stores JWT; redirects to Dashboard. Inline
  validation + error toast on bad credentials.
- **Dashboard** — `StatCard`s (overdue count, today's count, totals by status),
  `MemberDistribution` (per-member active counts), `TaskSummary` (today's tasks).
  Data from `useDashboard`.
- **Tasks** — list/table with filters (assignee, status, type). `TaskForm` modal for
  create/edit (assignee from `useUsers`, due-date picker). Delete with confirm.
  Swipe-to-complete (mobile) / click-to-complete (desktop) with optimistic feedback.
  `StatusBadge` per status including Overdue.
- **Recurring Tasks** — template list. `TemplateForm` with drag-reorderable
  `RotationOrder` (members from `useUsers`), frequency input. Generate-next-task
  button calls `POST /api/recurring-tasks/{id}/generate` and refreshes.
- **Profile** — current user info from `AuthContext`; logout.

---

## 5. UX / Motion

- **Mobile-first** (Tailwind breakpoints):
  - Mobile: single column, bottom nav, swipe-to-complete, full-screen modals.
  - Desktop: sidebar nav, multi-column dashboard, inline edits, hover states.
- **Framer Motion:** page transitions, task card enter/exit, drag for rotation
  reorder, button micro-interactions.
- **Feedback:** optimistic updates for completion; toast notifications for
  success/error across mutations.

---

## 6. Testing

- **Frontend (Vitest + RTL):**
  - Component tests: TaskForm validation, filter behavior, complete action, login form.
  - Hook tests: `useAuth` login/logout, `useTasks` fetch + optimistic complete
    (mocked `api/client`).
- **Backend (xUnit):**
  - `UserService` / `DashboardService` unit tests (mocked repos via NSubstitute).
  - API integration tests for `GET /api/users` and `GET /api/dashboard`
    (`WebApplicationFactory` + Testcontainers, matching existing API tests).
- **CI:** extend `.github/workflows/ci.yml` frontend job:
  `npm ci → lint → test → build`.

---

## 7. Delivery Order

1. Backend `GET /api/users` (TDD).
2. Backend `GET /api/dashboard` (TDD).
3. Scaffold `home-flow-ui` (Vite + TS) and the full stack (Tailwind, Shadcn/ui,
   Framer Motion, TanStack Query, Router, ESLint/Prettier, Vitest).
4. `api/client.ts`, `AuthContext`, routing, `ProtectedRoute`, Login.
5. Tasks page (list, filters, form, optimistic complete).
6. Recurring page (templates, rotation reorder, generate).
7. Dashboard page.
8. Profile page.
9. Motion + responsive polish.
10. Frontend tests + CI extension.

---

## Acceptance Criteria

- A user can log in, see the dashboard, create/edit/complete/delete one-off tasks,
  manage recurring templates with rotation order, generate the next rotation task,
  and view their profile — all against the live API.
- Completing a task updates the UI instantly and rolls back on server error.
- `GET /api/users` and `GET /api/dashboard` exist, are authed, and are covered by
  unit + integration tests.
- Frontend lints clean and component/hook tests pass in CI.
