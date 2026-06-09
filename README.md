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

API calls go to `/api`. In local dev, `proxy.conf.json` forwards to the Render dev API (or a local
backend if you run one from `workflowHub-backend`).

> Keep `ai-rules/`, `workflowHub-frontend/`, and `workflowHub-backend/` in the **same parent folder**.

## Stack

Angular 20 (standalone) · Tailwind CSS · PrimeNG · SCSS

## Branches

| Branch | Environment |
|---|---|
| `dev` | Netlify dev |
| `staging` | Netlify staging |
| `main` | Netlify production |
