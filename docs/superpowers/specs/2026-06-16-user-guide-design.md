# HomeFlow User Guide — Design Specification

**Date:** 2026-06-16
**Output:** `docs/user-guide.html`
**Audience:** End-users (household members, non-technical)

---

## Goal

A single self-contained HTML file that guides a new HomeFlow user through every feature of the app in a friendly, readable way. No frameworks, no external dependencies — opens in any browser offline.

---

## Layout

- **Header** — HomeFlow name + tagline ("Keep your home running smoothly")
- **Sticky left sidebar** — section links; active link highlights as the user scrolls
- **Main content column** — linear reading order, one section per feature
- **Responsive** — sidebar collapses on mobile; content stacks full-width

---

## Sections (in order)

1. **Getting Started** — how to log in; seeded demo accounts; first look at the app
2. **Dashboard** — what the stat cards mean; reading the member distribution; today's tasks list
3. **Tasks** — creating a one-off task; filters (assignee, status, type); completing, editing, deleting a task
4. **Recurring Tasks** — what a template is; creating a template with rotation order; generating the next task
5. **Profile** — viewing your account info; logging out
6. **FAQ** — accordion, 5 questions (see below)

---

## Content Pattern (per section)

Each section follows:
1. One-sentence intro explaining what the section is for
2. Numbered steps for each key action
3. Callout boxes (tip / note) for non-obvious behaviour

---

## FAQ Accordion

Questions covered:
- How do I mark a task as complete?
- What happens when a recurring task is generated?
- Who can complete a task?
- What does "Overdue" mean?
- How does the rotation work?

Each question is a clickable header; the answer expands/collapses with a smooth CSS transition. Multiple items can be open at once.

---

## Visual Style

- Colour palette: slate/neutral greys matching the app's Tailwind theme; one accent colour (indigo) for links and active states
- Typography: system font stack; 16px base, generous line-height
- Callout boxes: light background, left border accent, small icon (✓ tip, ⚠ note)
- No images/screenshots — uses clean numbered steps and emoji icons for section headers

---

## Implementation

- Pure HTML5 + embedded `<style>` + embedded `<script>` (no external files)
- Accordion: vanilla JS toggle on `<details>` or `<button>` + `aria-expanded`
- Scroll-spy: `IntersectionObserver` updates sidebar active state
- File location: `docs/user-guide.html`
