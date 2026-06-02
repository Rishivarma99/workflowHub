---
name: frontend-dev-server
description: Run, verify, and troubleshoot the Workflow Hub Angular dev server on port 4290. Use when starting the frontend, checking if it is up, reading its logs, or diagnosing why http://localhost:4290 is not responding.
---

# Frontend Dev Server

Operate the Workflow Hub Angular dev server. The app lives in `frontend/` and **must run on port 4290** (not Angular's default 4200).

## Quick facts

- App directory: `frontend/`
- Port: **4290**
- URL: http://localhost:4290/
- Start script: `.claude/hooks/start-frontend.sh` (also runs automatically on `SessionStart`)
- Logs: `/tmp/workflowhub-frontend-4290.log`
- PID file: `/tmp/workflowhub-frontend-4290.pid`

## Start / verify

1. Check if it is already up: `lsof -i :4290 -sTCP:LISTEN` or `curl -s -o /dev/null -w "%{http_code}" http://localhost:4290/`.
2. If up, report the URL and stop.
3. If not up, run `.claude/hooks/start-frontend.sh` from the repo root.
4. Fallback if the script fails or `node_modules` is missing: `npm install` in `frontend/`, then `npm start -- --port 4290`.

## Troubleshoot

- **Port not responding:** tail `/tmp/workflowhub-frontend-4290.log` for the Angular build output.
- **Stale PID:** if `lsof` shows nothing but the PID file exists, the previous process died — delete `/tmp/workflowhub-frontend-4290.pid` and restart.
- **Port already in use by something else:** identify with `lsof -i :4290` before killing anything.

## Rules

- Always use port **4290**. Never start a second server on 4200.
- Do not change the dev port unless the user explicitly asks.
