<div align="center">

# 🏠 HomeFlow

### Fair, automatic household task management for everyone under one roof

<p>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img alt="C#" src="https://img.shields.io/badge/C%23-raw%20Npgsql-239120?style=for-the-badge&logo=csharp&logoColor=white" />
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white" />
  <img alt="React" src="https://img.shields.io/badge/React-18%20%2B%20TS-61DAFB?style=for-the-badge&logo=react&logoColor=black" />
  <img alt="Clean Architecture" src="https://img.shields.io/badge/Clean-Architecture-FF6B6B?style=for-the-badge" />
</p>

<p>
  <a href="#-quick-start"><b>Quick Start</b></a> ·
  <a href="#-features"><b>Features</b></a> ·
  <a href="#-tech--dependencies"><b>Dependencies</b></a> ·
  <a href="docs/superpowers/specs/2026-06-14-home-flow-design.md"><b>📖 Full Design Spec »</b></a>
</p>

<img src="https://raw.githubusercontent.com/qgustavor/qgustavor/master/transparent.png" height="1" />

</div>

---

## ✨ What is HomeFlow?

> _As a household member, I want to manage and fairly distribute household tasks among all members, so that chores are rotated automatically and one-off tasks can be assigned with deadlines — ensuring nothing is forgotten and no one is overburdened._

**HomeFlow** is a household task manager for a single home of up to 4 members. It rotates recurring chores automatically, lets you assign one-off tasks with due dates, and keeps everyone accountable with a live dashboard of who is doing what.

This README is a quick tour. For the complete domain model, API reference, database schema, and architecture decisions, read the **[📘 full design specification](docs/superpowers/specs/2026-06-14-home-flow-design.md)**.

---

## 🎬 Features

> 🚧 **Demo GIFs are in the works!** Drop them into [`docs/assets/`](docs/assets/) and they'll light up the table below.

<table>
  <tr>
    <td width="50%" valign="top">

### 🔄 Recurring chore rotation
Define a chore once, set a frequency, and HomeFlow rotates it fairly through every household member — automatically.

<sub>📍 _GIF placeholder_ → `docs/assets/recurring-rotation.gif`</sub>

<!-- <img src="docs/assets/recurring-rotation.gif" width="100%" /> -->

  </td>
    <td width="50%" valign="top">

### 📌 One-off tasks with deadlines
Assign a quick task to anyone, set a due date, and track it to completion.

<sub>📍 _GIF placeholder_ → `docs/assets/one-off-tasks.gif`</sub>

<!-- <img src="docs/assets/one-off-tasks.gif" width="100%" /> -->

  </td>
  </tr>
  <tr>
    <td width="50%" valign="top">

### 📊 Accountability dashboard
See today's tasks, overdue counts, and how the workload is distributed across members at a glance.

<sub>📍 _GIF placeholder_ → `docs/assets/dashboard.gif`</sub>

<!-- <img src="docs/assets/dashboard.gif" width="100%" /> -->

  </td>
    <td width="50%" valign="top">

### 🔐 Auth & secure access
JWT-based login with seeded demo users, so you can explore instantly.

<sub>📍 _GIF placeholder_ → `docs/assets/auth.gif`</sub>

<!-- <img src="docs/assets/auth.gif" width="100%" /> -->

  </td>
  </tr>
</table>

---

## 🚀 Quick Start

```bash
# Spin up Postgres + API + UI
docker-compose up
```

| Service | URL |
|--------|-----|
| 🖥️  UI | http://localhost:3000 |
| ⚙️  API | http://localhost:5000 |
| 🐘 PostgreSQL | localhost:5432 |

**Demo login** — any seeded user (`Pedro`, `Maria`, `João`, `Ana`) with password `Password123!`

<details>
<summary><b>🛠️ Run backend & frontend separately</b></summary>

```bash
# Backend (.NET 10)
dotnet restore
dotnet run --project src/HomeFlow.API

# Frontend (React + Vite)
cd home-flow-ui
npm ci
npm run dev
```

Migrations and seed data run automatically on API startup.
</details>

---

## 🧩 Tech & Dependencies

### Backend
| Dependency | Purpose |
|-----------|---------|
| **.NET 10 / ASP.NET Web API** | HTTP API & hosting |
| **Npgsql** | Raw PostgreSQL access (no EF / Dapper / Mediator) |
| **PostgreSQL 16** | Data store |
| **BCrypt.Net** | Password hashing |
| **JWT Bearer** | Authentication tokens |
| **xUnit · NSubstitute · FluentAssertions** | Unit & integration testing |
| **Testcontainers** | Real-Postgres repository tests |

### Frontend (`home-flow-ui`)
| Dependency | Purpose |
|-----------|---------|
| **React 18 + TypeScript + Vite** | SPA framework & build |
| **Tailwind CSS** | Responsive, mobile-first styling |
| **Framer Motion** | Page transitions & micro-interactions |
| **Shadcn/ui** | Base UI components |
| **Vitest + React Testing Library** | Component & hook tests |
| **ESLint 9 + Prettier** | Linting & formatting |

### Tooling
**Docker Compose** (local dev) · **GitHub Actions** (CI) · **Railway** (deploy)

---

## 🏛️ Architecture

Strict **Clean Architecture** — no layer depends on a layer above it:

```
API  →  Application  →  Domain  ←  Infrastructure
```

- **Domain** — entities, enums, repository interfaces. Zero dependencies.
- **Application** — business services, DTOs, validation.
- **Infrastructure** — Npgsql repositories, JWT provider, migration runner.
- **API** — controllers, middleware, DI wiring.

> 🧪 Built with **TDD** — failing test first, then implementation. Every feature ships with tests.

📖 Full layer breakdown, domain model, database schema, and the complete API endpoint reference live in the **[design specification »](docs/superpowers/specs/2026-06-14-home-flow-design.md)**

---

## 📚 Documentation

- 📘 [Design Specification](docs/superpowers/specs/2026-06-14-home-flow-design.md) — architecture, domain model, API, DB schema
- 🗂️ [Session Logs](docs/sessions/) — how the codebase came to be, prompt by prompt
- 📮 [Postman Collection](docs/HomeFlow.postman_collection.json) — ready-to-import API requests

---

<div align="center">
<sub>Made with ☕ and clean architecture · HomeFlow</sub>
</div>
