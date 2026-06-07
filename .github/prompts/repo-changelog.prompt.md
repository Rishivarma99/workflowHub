---
description: Summarize application changes in a repo/branch between two dates — grouped by module, not by commit.
---

# Repo Changelog Summary (WF-193FE)

Produce a **holistic, module-oriented summary** of what changed in an application between two dates on a given branch. The output describes the app **before vs after** the period — never a commit-by-commit list.

## Step 0 — Collect Parameters

Ask once for any missing values before proceeding:

| Parameter | Required | Examples |
|-----------|----------|----------|
| **repo** | yes | `acme/web-app`, `/Users/me/projects/web-app` |
| **branch** | yes | `main`, `develop`, `release/2.0` |
| **from** | yes | `2025-01-01` |
| **to** | yes | `2025-03-31` |

## Workflow Checklist

```
- [ ] 1. Parse repo, branch, from, to
- [ ] 2. Run collect-changes.sh
- [ ] 3. Review manifest + module groupings
- [ ] 4. Analyze diffs (split by module if large)
- [ ] 5. Write report to docs/changelogs/
- [ ] 6. Reply with file path only — never paste full report in chat
```

## Step 1 — Collect Git Data

From the **project root**, run:

```bash
bash .github/workflows/wf-193fe/scripts/collect-changes.sh \
  "<repo>" "<branch>" "<from>" "<to>"
```

Script writes to `.github/workflows/wf-193fe/tmp/changelog-<slug>/`:
- `manifest.json` — refs, stats, module groupings, `reportPathRelative`
- `report-path.txt` — target path under `docs/changelogs/`
- `diff-stat.txt`, `files-by-module.json`, `patches/` (when manageable)

**Do not** build the report body from `git log --oneline` per commit.

## Step 2 — Understand the Period Diff

Period diff is **`base..tip`**:
- **base** = last commit on branch strictly before `from`
- **tip** = last commit on branch on or before end of `to`

Read `manifest.json` first, then module patches or targeted `git diff base..tip -- <path>`.

## Step 3 — Analysis Strategy

**Small change set** (< ~30 files): analyze directly.

**Large change set**: analyze module-by-module from `files-by-module.json`. Cap at 6 module groups; batch minor modules under `other/`.

Per module extract: Added, Changed, Removed, cross-cutting impacts. Skip formatting-only and lockfile noise unless runtime-relevant.

## Step 4 — Write Report (mandatory)

Path: `docs/changelogs/<repo-slug>-<branch>-<from>-<to>.md` (from `manifest.json` → `reportPathRelative`).

Follow `.github/workflows/wf-193fe/templates/output-template.md`:
1. Executive summary (2–4 sentences, product tone)
2. By module — primary structure
3. No commit-by-commit sections
4. Before → after language
5. Stats appendix only

## Step 5 — Delivery

1. **Write** the full report file before replying.
2. **Chat reply** = file path + 1–2 sentence headline only.

## Constraints

- **READ-ONLY** on the target repo
- **DO write** the changelog under this project's `docs/changelogs/`
- Focus on business/product impact by module
- Remote repos cache at `~/.cache/repo-changelog/` or `$REPO_CHANGELOG_CACHE`

## Error Handling

| Situation | Action |
|-----------|--------|
| Repo not found | Suggest local path or `gh auth login` |
| Branch missing | List `git branch -r` |
| Empty period | State "no changes in range" with refs |
