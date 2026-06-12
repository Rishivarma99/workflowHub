# Workflow Hub — Frontend

Angular 20 app for [Workflow Hub](workflowHub-prd-v2.md): publish, discover, and install IDE
workflows pinned to public GitHub repositories.

The .NET API lives in the sibling repo [`workflowHub-backend`](../workflowHub-backend/).

## Layout

| Path | What |
|---|---|
| `frontend/` | Angular 20 app — Tailwind + PrimeNG |
| `design-reference/` | Pointer to UI reference screens (sibling `workflowHub-design-reference/`) |
| `CLAUDE.md` | Guidance for Claude Code, incl. how to use the rules |

## Getting started

```bash
# sibling repos under the same parent folder
git clone <workflowhub-frontend-repo-url> workflowHub-frontend
git clone https://gitlab.pal.tech/rishi.alluri/ai-rules.git ai-rules

cd workflowHub-frontend/frontend
npm install
npm start -- --port 4290
```

Dev UI: http://localhost:4290/

API calls go to `/api`. The dev server (`proxy.conf.js`) forwards `/api` to the backend URL in
`API_PROXY_TARGET`, or `http://localhost:5031` when unset.

> Keep `ai-rules/`, `workflowHub-frontend/`, and `workflowHub-backend/` in the **same parent folder**.

## Local backend (.NET, no Docker)

From `workflowHub-backend`:

```bash
dotnet run --project WorkflowHub.Api
```

API: http://localhost:5031/ (see `launchSettings.json`).

Then start the frontend (no env var needed — proxy defaults to port 5031):

```bash
cd frontend
npm start -- --port 4290
```

Set `ConnectionStrings__DefaultConnection` and `Jwt__SigningKey` via user secrets or env vars.
`appsettings.Development.json` already allows CORS from `http://localhost:4290`.

## Docker — development

```bash
Copy-Item .env.docker.example .env.docker
# Edit .env.docker → set API_PROXY_TARGET to your backend URL
docker compose up --build
```

Open http://localhost:4290/

| Backend location | `API_PROXY_TARGET` in `.env.docker` |
|---|---|
| Local `dotnet run` on your machine | `http://host.docker.internal:5031` |
| Backend Docker (`workflowHub-backend`, port 8080) | `http://host.docker.internal:8080` |
| Remote dev API (Render, etc.) | `https://your-api-url` (no trailing slash) |

## Docker — staging / production (later)

```bash
Copy-Item .env.docker.staging.example .env.docker.staging
# set API_PROXY_TARGET, then:
docker compose -f docker-compose.staging.yml up --build

Copy-Item .env.docker.prod.example .env.docker.prod
# set API_PROXY_TARGET, then:
docker compose -f docker-compose.prod.yml up --build
```

## Stack

Angular 20 (standalone) · Tailwind CSS · PrimeNG · SCSS

## Branches

| Branch | Environment |
|---|---|
| `dev` | Netlify dev |
| `staging` | Netlify staging |
| `main` | Netlify production |
