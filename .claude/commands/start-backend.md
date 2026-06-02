---
description: Build and run the Workflow Hub .NET 8 backend API (WorkflowHub.Api) on http://localhost:5031.
allowed-tools: Bash, Read
---

# Start Backend API

Build and run the Workflow Hub backend (.NET 8, pinned via `backend/global.json`).

## Facts

- Solution directory: `backend/`
- API project: `backend/src/WorkflowHub.Api`
- Dev URL: http://localhost:5031/ (https: https://localhost:7100/)
- SDK: .NET 8 (do not bump — `backend/global.json` pins it)

## Steps

1. Check if the API is already up: `lsof -i :5031 -sTCP:LISTEN` or `curl -s -o /dev/null -w "%{http_code}" http://localhost:5031/`.
2. If already running, report the URL and stop.
3. From `backend/`, build first to surface errors clearly: `dotnet build`.
4. If the build is green, run the API: `dotnet run --project src/WorkflowHub.Api`.
5. Confirm http://localhost:5031/ responds and report:
   - Server status (running / failed)
   - The URL
   - Any build or startup errors

## Notes

- Run from the `backend/` directory so `global.json` is honored (.NET 8).
- Use the `http` profile / port **5031** unless the user asks for https (7100).
- Do not change the SDK version or ports unless explicitly asked.
