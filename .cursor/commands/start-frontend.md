---
name: start-frontend
description: Start or verify the Workflow Hub Angular dev server on port 4290.
---

# Start Frontend Dev Server

Ensure the Workflow Hub frontend is running at http://localhost:4290/.

## Steps

1. Check if port 4290 is listening (`lsof -i :4290 -sTCP:LISTEN` or `curl -s -o /dev/null -w "%{http_code}" http://localhost:4290/`).
2. If already running, report the URL and stop.
3. If not running, run `.cursor/hooks/start-frontend.sh` from the project root.
4. If the hook fails or `node_modules` is missing, run `npm install` in `frontend/`, then `npm start -- --port 4290` from `frontend/`.
5. Confirm http://localhost:4290/ responds and report:
   - Server status
   - Log file: `/tmp/workflowhub-frontend-4290.log`

Use port **4290** only. Do not start a second server on 4200.
