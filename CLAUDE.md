# CLAUDE.md — Workflow Hub

Guidance for Claude Code when working in this repository.

---

## ⚠️ Rule 0 — The coding rules in `ai-rules/` are MANDATORY

The coding-standards library lives in a **sibling folder**, `../ai-rules/` (a separate git repo,
kept open alongside this one in the same workspace). It is the single source of truth — edits are
made there and are always the latest; this repo does **not** keep a copy.
**Every code change must follow those rules.** They are not optional and not suggestions.

But: **do NOT load the whole `../ai-rules/` tree into context.** It is large and most of it is
irrelevant to any single task. Loading everything wastes context and degrades quality.

### How to use the rules (the loading protocol — follow this every time)

1. **Start at the master index:** `../ai-rules/RULES-INDEX.md`.
2. **Classify the task** — frontend (Angular), backend (.NET), or both.
3. **Open the matching domain sub-index** and the **Task → rules map** in `RULES-INDEX.md`,
   then load **only** the 2–5 rule files that match the task — plus the domain's
   **always-on** files (listed in the index).
4. If you're loading more than ~6 rule files for one task, re-scope — you're loading too much.
5. When a task spans both stacks, load the relevant slice from each domain, not both whole trees.

> Quick path: read `../ai-rules/RULES-INDEX.md` → find your task row → open only those files.

> **Setup note:** the rules are referenced by relative path, so keep `ai-rules/` checked out as a
> sibling of `workflowHub/` in the same parent folder (`.../GitHub/ai-rules` next to
> `.../GitHub/workflowHub`). If the rules aren't found at `../ai-rules/`, clone them there:
> `git clone https://gitlab.pal.tech/rishi.alluri/ai-rules.git ../ai-rules`

---

## What this project is

**Workflow Hub** — a web platform to **publish and discover IDE workflows that live in public
GitHub repos**, and to **install/convert them into a target IDE format on demand** (e.g. Cursor).

A published workflow is a *pinned reference* (repo URL + exact commit SHA + searchable metadata +
a small manifest) — **we do not store copies of workflow files**. Files are fetched from GitHub
only at install time, transformed, zipped, streamed to the user, and deleted.

Full product spec: [`workflowHub-prd-v2.md`](workflowHub-prd-v2.md). Read it for feature scope
(auth, publish, search, install/convert), the data model, and the API surface.

---

## Tech stack

| Layer | Stack |
|---|---|
| **Frontend** | Angular 20 (standalone, application builder), Tailwind CSS, PrimeNG, SCSS |
| **Backend** | .NET 8 (pinned via `backend/global.json`), CQRS, EF Core + Npgsql (PostgreSQL), JWT auth |
| **Search** | PostgreSQL full-text (MVP) |

> Use **PrimeNG** for rich UI components where it helps; **Tailwind** for layout/spacing/styling.
> .NET 8 is required — the machine default SDK may be newer, but `backend/global.json` pins 8.x.

---

## Repository layout

```
GitHub/
├── ai-rules/                 <- MANDATORY coding rules (separate sibling repo) — load selectively
│   └── RULES-INDEX.md        <- START HERE to pick rules
└── workflowHub/              <- THIS repo
    ├── CLAUDE.md             <- you are here
    ├── workflowHub-prd-v2.md <- product spec (source of truth for features)
    ├── frontend/             <- Angular 20 app (Tailwind + PrimeNG)
    │   └── src/app/
    │       ├── core/         <- singletons: services, guards, interceptors, config
    │       ├── shared/       <- reusable UI, pipes, directives, models, types
    │       └── features/     <- feature areas (auth, workflows, home)
    └── backend/              <- .NET 8 solution (4-layer CQRS)
        ├── global.json       <- pins .NET 8 SDK
        └── src/
            ├── WorkflowHub.Api/          <- thin controllers + composition (Program.cs stays thin)
            ├── WorkflowHub.Application/  <- CQRS commands/queries/handlers, services
            ├── WorkflowHub.Data/         <- EF Core: DbContext, entities, repositories, migrations
            └── WorkflowHub.Common/       <- shared contracts (ApiResponse<T>), constants, errors
```

Dependency direction (backend): `Api → Application → Data → Common`. Nothing depends on `Api`.

---

## Commands

**Frontend** (run from `frontend/`):
```bash
npm install
npm start            # ng serve — dev server
npm run build        # production build (must stay green)
npm test             # unit tests
```

**Backend** (run from `backend/`):
```bash
dotnet build         # must stay green; uses .NET 8 via global.json
dotnet run --project src/WorkflowHub.Api
```

---

## Working agreements

- **Rules first.** Before writing code, load the relevant `ai-rules/` files via `RULES-INDEX.md`.
  If a rule and a quick hack conflict, the rule wins — or flag it.
- **Match the PRD** for feature behavior and the data model; match `ai-rules/` for *how* to build it.
- **Structure is scaffolded, screens are not.** UI screens are being designed separately. Build
  features against the existing core/shared/features structure when designs land.
- **Keep `Program.cs` thin** and respect layer boundaries (see backend architecture rules).
- **Project prefix is `wh`** for Angular selectors/components (e.g. `wh-root`).
- Don't commit secrets (JWT keys, Google client secret) — see the PRD's non-functional requirements.
- When you add/rename/remove a rule file, update `../ai-rules/RULES-INDEX.md`.
