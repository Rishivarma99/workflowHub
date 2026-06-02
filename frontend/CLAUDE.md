# Frontend dev server

> Path-scoped guidance — auto-loaded by Claude Code when working under `frontend/`
> (the equivalent of Cursor's `globs: frontend/**`).

The Angular app in `frontend/` runs on **port 4290**, not the default 4200.

- Dev URL: http://localhost:4290/
- Start manually: `cd frontend && npm start -- --port 4290`
- Auto-start: `.claude/hooks/start-frontend.sh` runs on Claude Code `SessionStart`
- Logs: `/tmp/workflowhub-frontend-4290.log`

When testing UI changes or sharing links, always use port **4290**.
