---
name: frontend-dev
description: Starts, verifies, and troubleshoots the Workflow Hub Angular dev server on port 4290. Use when the user asks to run the frontend, preview UI changes, fix dev server issues, or mentions port 4290.
paths:
  - frontend/**
---

# Frontend Dev Server

Manage the Workflow Hub Angular app in `frontend/` on port **4290**.

## Quick reference

| Item | Value |
|------|-------|
| URL | http://localhost:4290/ |
| Start | `cd frontend && npm start -- --port 4290` |
| Hook | `.cursor/hooks/start-frontend.sh` |
| Logs | `/tmp/workflowhub-frontend-4290.log` |
| PID | `/tmp/workflowhub-frontend-4290.pid` |

## Start or verify

1. Check port 4290: `lsof -i :4290 -sTCP:LISTEN` or `curl -s -o /dev/null -w "%{http_code}" http://localhost:4290/`
2. If running, report the URL and skip startup.
3. If not running, run `.cursor/hooks/start-frontend.sh` from the project root.
4. If hook fails, ensure `frontend/node_modules` exists (`npm install` if missing), then run `npm start -- --port 4290` from `frontend/`.
5. Wait up to 15 seconds for http://localhost:4290/ to respond.
6. Report status, URL, and log path.

## Troubleshooting

- **Port in use but app broken**: read `/tmp/workflowhub-frontend-4290.log`, kill stale process, restart.
- **Build errors**: run `npm run build` in `frontend/` to surface compile issues.
- **Wrong port**: always use **4290**, not Angular's default 4200.

## After UI changes

Confirm hot reload is active (`ng serve` watch mode). Share http://localhost:4290/ for manual verification.

Do not change the dev port unless the user explicitly asks.
