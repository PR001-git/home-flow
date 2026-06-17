# Task B7: useUsers + useTasks Query Hooks

**Date:** 2026-06-16
**Branch:** feat/frontend
**Session file:** docs/sessions/2026-06-16-004-task-b7-query-hooks.md

---

## Prompts & Interactions

### 1. Implement Task B7

**Prompt:**
> You are implementing Task B7: useUsers + useTasks query hooks (with optimistic complete), in the HomeFlow repo at C:\Pedro\code\home-flow, on git branch feat/frontend.

**What Claude did:**
- Read task brief at `.git/sdd/task-B7-brief.md`
- Read existing `src/types/index.ts` to confirm `TaskStatus`/`TaskType` are `const` objects, not enums
- Read `src/api/client.ts` to confirm `apiClient` interface (`get`, `post`, `put`, `patch`, `del`)
- Wrote `src/hooks/useTasks.test.tsx` (failing test — `useTasks` module not found)
- Ran `npm test -- useTasks` → confirmed FAIL (module resolution error)
- Created `src/hooks/useUsers.ts` with `useQuery` for `/api/users`
- Created `src/hooks/useTasks.ts` with: `useTasks`, `useCreateTask`, `useUpdateTask`, `useDeleteTask`, and `useCompleteTask` (optimistic update via `onMutate`/`onError`/`onSettled`)
- Ran `npm test -- useTasks` → PASS (1/1)
- Ran `npm run build` → PASS (no TS errors, vite bundle produced)
- Ran `npm run lint` → PASS (no ESLint errors)
- Committed all three files with message `feat(ui): add useUsers and useTasks query hooks with optimistic complete` (SHA: dcf69db)

**Outcome:**
Three new files committed; `useCompleteTask` implements optimistic status update across all `['tasks']` cache entries with rollback on error and invalidation of `['tasks']` + `['dashboard']` on settled.

---

## Session summary

Goal was to add React Query hooks for tasks and users, following TDD (write failing test, implement, pass). `useCompleteTask` uses optimistic updates: snapshot all `['tasks']` cache entries in `onMutate`, set status to `TaskStatus.Completed` for the matching task, roll back in `onError`, and invalidate both `['tasks']` and `['dashboard']` in `onSettled`. All other mutations (`create`, `update`, `delete`) invalidate those same two query keys on success. Build, lint, and tests all pass clean.
