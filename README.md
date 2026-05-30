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
| `ai-rules/` | Mandatory coding-standards library (git submodule) |
| `CLAUDE.md` | Guidance for Claude Code, incl. how to use `ai-rules/` |

## Getting started

```bash
# clone with the rules submodule
git clone <repo-url>
cd workflowHub
git submodule update --init --recursive

# frontend
cd frontend && npm install && npm start

# backend (uses .NET 8 via backend/global.json)
cd backend && dotnet build && dotnet run --project src/WorkflowHub.Api
```

## Stack

- **Frontend:** Angular 20 (standalone), Tailwind CSS, PrimeNG, SCSS
- **Backend:** .NET 8, CQRS, EF Core + Npgsql, PostgreSQL full-text search, JWT
