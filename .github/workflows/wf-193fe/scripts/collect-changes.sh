#!/usr/bin/env bash
# Collect aggregated git changes for a date range on a branch.
# Usage: collect-changes.sh <repo> <branch> <from-date> <to-date>
set -euo pipefail

REPO="${1:?repo required (local path or owner/repo)}"
BRANCH="${2:?branch required}"
FROM_DATE="${3:?from date required (YYYY-MM-DD)}"
TO_DATE="${4:?to date required (YYYY-MM-DD)}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../../../.." && pwd)"
CACHE_ROOT="${REPO_CHANGELOG_CACHE:-$HOME/.cache/repo-changelog}"
OUTPUT_ROOT="$PROJECT_ROOT/.github/workflows/wf-193fe/tmp"

normalize_date_end() {
  local d="$1"
  if [[ "$d" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}$ ]]; then
    echo "${d}T23:59:59"
  else
    echo "$d"
  fi
}

normalize_date_start() {
  local d="$1"
  if [[ "$d" =~ ^[0-9]{4}-[0-9]{2}-[0-9]{2}$ ]]; then
    echo "${d}T00:00:00"
  else
    echo "$d"
  fi
}

FROM_ISO="$(normalize_date_start "$FROM_DATE")"
TO_ISO="$(normalize_date_end "$TO_DATE")"

slugify() {
  echo "$1" | tr '/:' '--' | tr -cd 'a-zA-Z0-9._-' | tr '[:upper:]' '[:lower:]'
}

REPO_SLUG="$(slugify "$REPO")"
OUTPUT_DIR="$OUTPUT_ROOT/changelog-${REPO_SLUG}-$(slugify "$BRANCH")-$(slugify "$FROM_DATE")-$(slugify "$TO_DATE")"
mkdir -p "$OUTPUT_DIR/patches"

resolve_repo_path() {
  if [[ -d "$REPO" && -d "$REPO/.git" ]]; then
    echo "$(cd "$REPO" && pwd)"
    return
  fi

  if [[ "$REPO" =~ ^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$ ]]; then
    local cache_path="$CACHE_ROOT/$(slugify "$REPO")"
    if [[ -d "$cache_path/.git" ]]; then
      git -C "$cache_path" fetch --all --prune --quiet 2>/dev/null || true
      echo "$cache_path"
      return
    fi
    mkdir -p "$CACHE_ROOT"
    if command -v gh >/dev/null 2>&1; then
      gh repo clone "$REPO" "$cache_path" -- --quiet
    else
      git clone "https://github.com/${REPO}.git" "$cache_path" --quiet
    fi
    echo "$cache_path"
    return
  fi

  echo "ERROR: Cannot resolve repo '$REPO'. Use a local path with .git or owner/repo." >&2
  exit 1
}

REPO_PATH="$(resolve_repo_path)"
cd "$REPO_PATH"

git fetch origin "$BRANCH" --quiet 2>/dev/null || git fetch --all --quiet 2>/dev/null || true

if git show-ref --verify --quiet "refs/heads/$BRANCH"; then
  REF="refs/heads/$BRANCH"
elif git show-ref --verify --quiet "refs/remotes/origin/$BRANCH"; then
  REF="refs/remotes/origin/$BRANCH"
else
  echo "ERROR: Branch '$BRANCH' not found locally or on origin." >&2
  git branch -r | head -20 >&2
  exit 1
fi

BASE="$(git rev-list -n 1 --before="$FROM_ISO" "$REF" 2>/dev/null || true)"
TIP="$(git rev-list -n 1 --before="$TO_ISO" "$REF" 2>/dev/null || true)"

if [[ -z "$BASE" ]]; then
  BASE="$(git rev-list --max-parents=0 "$REF" | tail -1)"
fi
if [[ -z "$TIP" ]]; then
  TIP="$(git rev-parse "$REF")"
fi

if [[ "$BASE" == "$TIP" ]]; then
  COMMITS_IN_RANGE=0
else
  COMMITS_IN_RANGE="$(git rev-list --count "${BASE}..${TIP}" 2>/dev/null || echo 0)"
fi

git diff "${BASE}..${TIP}" --stat > "$OUTPUT_DIR/diff-stat.txt" 2>/dev/null || echo "(no diff)" > "$OUTPUT_DIR/diff-stat.txt"
git diff "${BASE}..${TIP}" --name-status > "$OUTPUT_DIR/files-name-status.txt" 2>/dev/null || true
git diff "${BASE}..${TIP}" --name-only > "$OUTPUT_DIR/files-changed.txt" 2>/dev/null || true

NUMSTAT="$(git diff "${BASE}..${TIP}" --numstat 2>/dev/null || true)"
ADDED=0
DELETED=0
if [[ -n "$NUMSTAT" ]]; then
  while IFS=$'\t' read -r a d _; do
    [[ "$a" == "-" ]] && continue
    ADDED=$((ADDED + a))
    DELETED=$((DELETED + d))
  done <<< "$NUMSTAT"
fi

CONTRIBUTORS="$(git log "${BASE}..${TIP}" --format='%an' 2>/dev/null | sort -u | paste -sd ', ' - || echo "")"

export OUTPUT_DIR REPO REPO_PATH BRANCH FROM_DATE TO_DATE FROM_ISO TO_ISO
export BASE TIP COMMITS_IN_RANGE ADDED DELETED CONTRIBUTORS PROJECT_ROOT

python3 <<'PY'
import json
import re
import subprocess
from collections import defaultdict
from pathlib import Path

import os

output_dir = Path(os.environ["OUTPUT_DIR"])
repo_path = os.environ["REPO_PATH"]
base = os.environ["BASE"]
tip = os.environ["TIP"]
max_patch_files = 80


def slugify(s: str) -> str:
    return re.sub(r"[^a-zA-Z0-9._-]", "-", s.replace("/", "-").replace(":", "-")).lower()


def repo_slug(repo: str, repo_path: str) -> str:
    if "/" in repo and not repo.startswith("/"):
        return slugify(repo.replace("/", "-"))
    return slugify(Path(repo_path).name)


project_root = Path(os.environ["PROJECT_ROOT"])
branch_slug = slugify(os.environ["BRANCH"])
from_slug = slugify(os.environ["FROM_DATE"])
to_slug = slugify(os.environ["TO_DATE"])
report_rel = f"docs/changelogs/{repo_slug(os.environ['REPO'], repo_path)}-{branch_slug}-{from_slug}-{to_slug}.md"
report_path = project_root / report_rel


def git_diff(args):
    return subprocess.run(
        ["git", "-C", repo_path, "diff", f"{base}..{tip}", *args],
        capture_output=True,
        text=True,
    )


changed = [
    l.strip()
    for l in (output_dir / "files-changed.txt").read_text().splitlines()
    if l.strip()
]

name_status = (output_dir / "files-name-status.txt").read_text().splitlines()
added_files = sum(1 for l in name_status if l.startswith("A"))
deleted_files = sum(1 for l in name_status if l.startswith("D"))
modified_files = len(changed) - added_files - deleted_files
if modified_files < 0:
    modified_files = 0

modules = defaultdict(list)
for f in changed:
    if f.startswith("src/"):
        parts = f.split("/")
        mod = f"src/{parts[1]}" if len(parts) > 1 else "src"
    elif "/" in f:
        mod = f.split("/")[0]
    else:
        mod = "root"
    modules[mod].append(f)

(output_dir / "files-by-module.json").write_text(json.dumps(dict(modules), indent=2))

patches_generated = len(changed) <= max_patch_files and len(changed) > 0
patches_dir = output_dir / "patches"
if patches_generated:
    for mod, files in modules.items():
        safe = slugify(mod)
        r = git_diff(["--", *files])
        (patches_dir / f"{safe}.patch").write_text(r.stdout or "")

manifest = {
    "repo": os.environ["REPO"],
    "repoPath": repo_path,
    "branch": os.environ["BRANCH"],
    "from": os.environ["FROM_DATE"],
    "to": os.environ["TO_DATE"],
    "fromIso": os.environ["FROM_ISO"],
    "toIso": os.environ["TO_ISO"],
    "base": base,
    "tip": tip,
    "baseShort": subprocess.check_output(
        ["git", "-C", repo_path, "rev-parse", "--short", base], text=True
    ).strip(),
    "tipShort": subprocess.check_output(
        ["git", "-C", repo_path, "rev-parse", "--short", tip], text=True
    ).strip(),
    "commitsInRange": int(os.environ["COMMITS_IN_RANGE"]),
    "filesChanged": len(changed),
    "filesAdded": added_files,
    "filesModified": modified_files,
    "filesDeleted": deleted_files,
    "linesAdded": int(os.environ["ADDED"]),
    "linesDeleted": int(os.environ["DELETED"]),
    "contributors": os.environ["CONTRIBUTORS"],
    "outputDir": str(output_dir),
    "reportPath": str(report_path),
    "reportPathRelative": report_rel,
    "patchesGenerated": patches_generated,
    "modules": sorted(modules.keys()),
}
(output_dir / "manifest.json").write_text(json.dumps(manifest, indent=2))
(output_dir / "report-path.txt").write_text(report_rel + "\n")
PY

TOTAL_FILES="$(wc -l < "$OUTPUT_DIR/files-changed.txt" | tr -d ' ')"
[[ -z "$TOTAL_FILES" || ! -s "$OUTPUT_DIR/files-changed.txt" ]] && TOTAL_FILES=0

echo "$OUTPUT_DIR"
echo "---"
echo "Base: $(git rev-parse --short "$BASE") → Tip: $(git rev-parse --short "$TIP")"
echo "Commits in range: $COMMITS_IN_RANGE | Files: $TOTAL_FILES | +$ADDED / -$DELETED lines"
echo "Output: $OUTPUT_DIR/manifest.json"
echo "Report: docs/changelogs/$(basename "$(cat "$OUTPUT_DIR/report-path.txt" 2>/dev/null || echo '')")"
