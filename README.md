# Workflow Hub

Publish and discover IDE workflows that live in public GitHub repos, and install/convert them
into a target IDE format (e.g. Cursor) on demand. A published workflow is a *pinned reference*
to a public repo (URL + commit SHA + metadata + manifest) — source files are fetched, transformed,
and zipped only at install time, never stored permanently.

See [`workflowHub-prd-v2.md`](workflowHub-prd-v2.md) for the full product spec.

## Monorepo layout

| Path | What |
|---|---|
| `frontend/` | Angular 20 app — Tailwind + PrimeNG |
| `backend/` | .NET 8 solution — 4-layer CQRS, EF Core + PostgreSQL, JWT auth |
| `CLAUDE.md` | Guidance for Claude Code, incl. how to use the rules |

The mandatory coding-standards library lives in a **separate sibling repo**, `../ai-rules/`, kept
checked out next to this one. It is referenced by relative path (no copy is stored here), so edits
in `ai-rules/` are always the latest. See `CLAUDE.md` for the rule-loading protocol.

## Getting started

```bash
# from a parent folder, check out both repos side by side
git clone https://gitlab.pal.tech/rishi.alluri/ai-rules.git    # the rules (sibling)
git clone <workflowhub-repo-url> workflowHub
cd workflowHub

# frontend
cd frontend && npm install && npm start

# backend (uses .NET 8 via backend/global.json)
cd backend && dotnet build && dotnet run --project src/WorkflowHub.Api
```

> Keep `ai-rules/` and `workflowHub/` in the **same parent folder** so `../ai-rules/` resolves.

## Stack

- **Frontend:** Angular 20 (standalone), Tailwind CSS, PrimeNG, SCSS
- **Backend:** .NET 8, CQRS, EF Core + Npgsql, PostgreSQL full-text search, JWT
