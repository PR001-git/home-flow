# Task B1: Scaffold Vite + React + TS with Tailwind and Vitest

**Date:** 2026-06-16
**Branch:** feat/frontend
**Session file:** docs/sessions/2026-06-16-003-task-b1-scaffold-frontend.md

---

## Prompts & Interactions

### 1. Scaffold the frontend SPA

**Prompt:**
> You are implementing Task B1: Scaffold Vite + React + TS, Tailwind, dependencies, in the HomeFlow repo at C:\Pedro\code\home-flow, on git branch feat/frontend. [full task brief followed]

**What Claude did:**
- Read task brief from `.git/sdd/task-B1-brief.md`
- Scaffolded `home-flow-ui/` with `npm create vite@latest home-flow-ui -- --template react-ts`
- Installed runtime deps: `@tanstack/react-query`, `react-router-dom`, `framer-motion`, `clsx`, `tailwind-merge`
- Installed dev deps: `tailwindcss`, `postcss`, `autoprefixer`, `vitest`, `@testing-library/react`, `@testing-library/jest-dom`, `@testing-library/user-event`, `jsdom`, `@types/node`, `prettier`, `eslint-config-prettier`
- Discovered Tailwind v4 was installed (not v3); `npx tailwindcss init -p` doesn't work in v4 — created `tailwind.config.js` and `postcss.config.js` manually
- Updated `postcss.config.js` to use `@tailwindcss/postcss` (required in v4) after build failed with the old `tailwindcss` PostCSS plugin
- Overwrote `vite.config.ts` using `defineConfig` from `vitest/config` (not `vite`) to expose the `test` key to TypeScript
- Added `"types": ["node", "vitest/globals"]` to `tsconfig.node.json`
- Overwrote `src/index.css` with Tailwind v3-style directives (`@tailwind base/components/utilities`)
- Created `src/test/setup.ts` importing `@testing-library/jest-dom`
- Created `src/test/smoke.test.ts` with a trivial arithmetic assertion
- Updated `package.json` scripts to add `test`, `test:watch`, and corrected `preview` port
- Fixed rolldown native binding issue (Node 22.11.0 below required 22.12.0) by explicitly installing `@rolldown/binding-win32-x64-msvc` as a devDependency
- Downgraded `jsdom` from v27 to v25 to fix ESM incompatibility (`@csstools/css-calc` ESM conflict in worker pool); added `pool: 'vmThreads'` to vitest config
- Ran `npm run build && npm test` — both passed
- Committed `home-flow-ui/` as a single commit from repo root

**Outcome:**
`home-flow-ui/` is scaffolded, builds successfully, and the Vitest smoke test passes (1/1). Commit d5a18e2 lands on branch `feat/frontend`.

---

## Session summary

The goal was to scaffold the Vite + React + TypeScript SPA under `home-flow-ui/` with Tailwind CSS, Vitest, and standard dependencies as specified in the task brief. The main complications were: (1) Tailwind v4 was installed instead of v3, requiring `@tailwindcss/postcss` as the PostCSS plugin; (2) Node.js 22.11.0 is below the rolldown 1.0.3 minimum, requiring the Windows native binding to be installed explicitly; (3) jsdom 27 has an ESM incompatibility that required downgrading to jsdom 25. The `vite.config.ts` imports `defineConfig` from `vitest/config` instead of `vite` to satisfy TypeScript's type checking for the `test` block. Build and tests are green; the dev server proxies `/api` to `http://localhost:5000`.
