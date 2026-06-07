# My Workflows Module

**Status:** Built (MVP) · **Last updated:** 6 Jun 2026
**Related PRD:** publish (§4.2), owner actions (WF-8), API (§6).

---

## 1. What it is

**My workflows** is the signed-in author's dashboard for workflows they have published — view stats,
open detail, and delete. Re-publish / metadata edit is post-MVP (Edit opens detail for now).

## 2. Route

`/workflows/mine` — sidebar **My workflows**.

## 3. Screen (design reference)

- Page header + **New workflow** → `/workflows/create`
- Stat strip: **Published**, **Total downloads**, **Files indexed**
- List rows: title, Public badge, description, IDE badge, file count, downloads, relative updated time
- Row overflow menu: **Edit** (detail), **View** (detail), **Delete** (confirm + API)

## 4. API

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/api/workflows/mine` | Current user's workflows + summary stats |
| `DELETE` | `/api/workflows/{id}` | Owner-only delete (cascades components) |

## Definition of done

- [x] Lists only the signed-in user's workflows, newest first.
- [x] Summary stats match list data.
- [x] Delete removes workflow and refreshes stats.
- [x] Empty state with CTA to publish.
- [x] Loading / error states handled.
