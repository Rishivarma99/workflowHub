# Discover Module

**Status:** Decisions landing · **Last updated:** 31 May 2026
**Related PRD:** browse + search (§4.5), component model (§0, §4.3), install trigger (§4.4).

---

## 1. What it is

The **Discover** module is where users **find workflows** — browse them, search them, filter by
component type, and open a workflow's detail page to install/convert it.

A **workflow is a bundle of components** (rules, commands, subagents, hooks, skills, …). When an
author publishes a repo, we fetch it once and build a per-file record describing **what each file
mainly does**; search runs over those records across **all** component types — not commands only.

## 2. Access — LOGIN REQUIRED 🔒

**The entire Discover module requires login.** Browse, search, and detail are all behind auth — a
logged-out user is redirected to Google sign-in first.

> ⚠️ This **diverges from the current PRD**, which says browse/search are public (SR-6, WF-9).
> Decision: login-gated. The PRD should be updated to match (flagged in §8).

## 3. Placement

```
App sidebar
  ├── Discover        <- this module (default landing after login)
  ├── Settings
  └── ...
```

## 4. Screens

| Screen | Purpose |
|---|---|
| **Browse / Search** | One screen: a search bar with quick filter chips below it, and a paginated list of workflow cards. With no query it's "browse all"; with a query it's "search results". |
| **Workflow detail** | Everything about one workflow: metadata, components (manifest), source repo/commit, install count, and the **Install/Convert** action. |

## 5. Search & filters (the core of this module)

### 5.1 Search bar
A single search box. It matches across workflow metadata **and** every component's
`summary` / `capabilities` / `commands` / `keywords` (the "what each file does" data built at
publish). Not confined to commands.

### 5.2 Filter chips (below the search bar, apply immediately)
Multi-select toggle buttons — any combination can be active; results update instantly:

```
[ Rules ]  [ Commands ]  [ Subagents ]  [ Hooks ]  [ Skills ]
```

- The component types are exactly these **five**: `rule`, `command`, `subagent`, `hook`, `skill`.
- Each chip filters to workflows containing that component type.
- Multi-select = OR within the chosen types (e.g. Rules + Hooks → workflows with rules *or* hooks).
- Each chip shows a **facet count** (e.g. `Rules 12`) so the user sees what's available.

> **Files that aren't one of the five types** (e.g. `mcp.json`, `README.md`, helper scripts) are
> **not indexed as searchable components** and have no chip. They are still part of the repo and are
> included when the whole workflow is installed — they're just supporting files, not first-class,
> searchable/filterable components.

### 5.3 How matches are shown (workflow-centric, with highlight)
We do **not** show a standalone component (a lone skill/rule) as a result. A component always lives
inside a workflow, so:
- A match surfaces the **parent workflow card**, with the **matched component highlighted** on it
  (e.g. a "matched: skill `code-review` ▸ `skills/review.md`" line on the card).
- Clicking goes to the workflow detail, ideally scrolled to / highlighting that component.

### 5.4 Paging
**Pagination** (numbered pages / prev-next), not infinite scroll. Page size TBD (§8).

### 5.5 Empty states
- No workflows at all → friendly empty state.
- No results for query/filters → "nothing matches" + a way to clear filters.

## 6. Workflow card — proposed contents

No ratings. **Install/download count** instead. Proposed card:

```
┌──────────────────────────────────────────────────────────┐
│  Workflow Title                                            │
│  One- or two-line description (truncated)…                 │
│                                                            │
│  ● 3 Rules   ● 2 Commands   ● 1 Hook        (type badges)  │
│  #tag  #tag  #tag                                          │
│                                                            │
│  👤 Author name · avatar          ⬇ 1,240 installs         │
│                                                            │
│  — (search only) matched: skill `code-review` ▸ review.md  │
└──────────────────────────────────────────────────────────┘
```

Fields shown:
- **Title** + **short description**
- **Component-type badges with counts** (the workflow's `component_types[]` + per-type counts) — this
  is the at-a-glance "what's inside"
- **Tags** (a few, overflow hidden)
- **Author** (name + avatar)
- **Install count** (replaces rating)
- **Matched-component highlight** row — only on search results
- Whole card is clickable → detail. (Optional quick "Install" on hover — §8.)

## 7. Install model — whole workflow only

**No per-file / partial download.** A user installs the **entire workflow**, not individual files —
components in a workflow are usually related, so a single file in isolation is rarely useful. The
detail page's Install/Convert action bundles the whole workflow (Install module handles the rest).

## 8. Decisions — resolved & remaining

**Resolved**
1. ✅ **Component types = the five only:** `rule`, `command`, `subagent`, `hook`, `skill`. No
   `mcp_plugin`/`other`. Non-matching files are bundled at install but not indexed (see §5.2).
2. ✅ **Update the PRD** to match these decisions (login-gated, install_count, 5-type enum). Doing.
3. ✅ **Install count** — add `install_count` (int) to the `Workflow` model, incremented per install,
   shown on the card. Replaces ratings.

**Remaining (sensible defaults applied; adjust anytime)**
4. **Page size** — default **12** per page.
5. **Install trigger** — from the **detail page** only (whole workflow). The card just *shows* the
   install count; it doesn't install.
6. **Default sort** (no query) — **most-installed** first.

---

## Definition of done (firms up after §8)

- [ ] Discover is login-gated; logged-out users are sent to sign-in.
- [ ] Browse + search on one screen with instant multi-select component-type filter chips (+ facet counts).
- [ ] Search matches across all component types and surfaces the **parent workflow** with the matched component highlighted.
- [ ] Workflow cards show title, description, type badges+counts, tags, author, and install count.
- [ ] Pagination works; empty/loading/error states handled.
- [ ] Detail page installs the **whole** workflow (no per-file download).
