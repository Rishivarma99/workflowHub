#!/usr/bin/env bash
# Starts the Angular dev server on port 4290 when a Cursor session begins.

set -euo pipefail

PORT=4290
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
FRONTEND_DIR="${PROJECT_ROOT}/frontend"
PID_FILE="/tmp/workflowhub-frontend-${PORT}.pid"
LOG_FILE="/tmp/workflowhub-frontend-${PORT}.log"

escape_for_json() {
  local s="$1"
  s="${s//\\/\\\\}"
  s="${s//\"/\\\"}"
  s="${s//$'\n'/\\n}"
  s="${s//$'\r'/\\r}"
  s="${s//$'\t'/\\t}"
  printf '%s' "$s"
}

emit_context() {
  printf '{\n  "additional_context": "%s"\n}\n' "$(escape_for_json "$1")"
}

# sessionStart sends JSON on stdin; consume it so the pipe does not block.
cat > /dev/null || true

is_port_in_use() {
  lsof -i ":${PORT}" -sTCP:LISTEN -t >/dev/null 2>&1
}

is_pid_running() {
  local pid="$1"
  [[ -n "$pid" ]] && kill -0 "$pid" 2>/dev/null
}

if is_port_in_use; then
  emit_context "Workflow Hub frontend is already running at http://localhost:${PORT}."
  exit 0
fi

if [[ -f "$PID_FILE" ]]; then
  existing_pid="$(cat "$PID_FILE")"
  if is_pid_running "$existing_pid"; then
    emit_context "Workflow Hub frontend is already running at http://localhost:${PORT} (pid ${existing_pid})."
    exit 0
  fi
  rm -f "$PID_FILE"
fi

if [[ ! -d "${FRONTEND_DIR}/node_modules" ]]; then
  emit_context "Workflow Hub frontend was not started: run 'npm install' in frontend/ first, then restart the session or run 'npm start -- --port ${PORT}' from frontend/."
  exit 0
fi

if ! command -v npm >/dev/null 2>&1; then
  emit_context "Workflow Hub frontend was not started: npm was not found on PATH."
  exit 0
fi

cd "$FRONTEND_DIR"
nohup npm start -- --port "$PORT" >> "$LOG_FILE" 2>&1 &
server_pid=$!
echo "$server_pid" > "$PID_FILE"
disown "$server_pid"

ready=false
for _ in $(seq 1 15); do
  if is_port_in_use; then
    ready=true
    break
  fi
  if ! is_pid_running "$server_pid"; then
    break
  fi
  sleep 1
done

if [[ "$ready" == true ]]; then
  emit_context "Started Workflow Hub frontend at http://localhost:${PORT}. Logs: ${LOG_FILE}"
elif is_pid_running "$server_pid"; then
  emit_context "Workflow Hub frontend is starting on http://localhost:${PORT}. Logs: ${LOG_FILE}"
else
  emit_context "Failed to start Workflow Hub frontend on port ${PORT}. Check ${LOG_FILE} for details."
fi

exit 0
