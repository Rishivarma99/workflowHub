# Discover Module

**Status:** Decisions landing · **Last updated:** 6 Jun 2026
**Related PRD:** Workflow marketplace (§4.5), component model (§4.3), install trigger (§4.4).

---

## 1. What it is

The **Discover** module is the **Workflow Marketplace** — where users find **complete solutions**
(outcomes), not individual rules or skills.

User intent:

```text
I want a workflow that does X.
```

Examples: PR Review Workflow, AI Research Workflow, Documentation Generator Workflow.

A workflow is a bundle of agent assets (rules, commands, subagents, hooks, skills). Those assets are
indexed at publish and may **boost workflow relevance**, but Discover **never** returns individual
assets as top-level results. For that, users go to **Agent Assets** (see
[`agent-assets-module.md`](agent-assets-module.md)).

## 2. Access — LOGIN REQUIRED

The entire Discover module requires login. Browse, search, and detail are all behind auth.

## 3. Placement

```
App sidebar
  ├── Discover        ← this module (default landing after login)
  ├── Agent Assets    ← separate marketplace for reusable assets
  ├── Settings
  └── …
```

## 4. Screens

| Screen | Purpose |
|---|---|
| **Discover home** | Workflow search, trending / new / browse sections, paginated workflow results. |
| **Browse all** | Paginated workflow list with optional “contains type” filters. |
| **Workflow detail** | Metadata, manifest, repo/commit, star + download counts, copy-clone get flow. |

## 5. Search (workflows only)

### 5.1 Search bar

Placeholder: `Search workflows…`

Helper text:

```text
Looking for rules, commands, skills, hooks, or agents?
Browse Agent Assets →
```

### 5.2 Search scope (server-side)

Matches across:

- Workflow title, description, tags, author
- Workflow metadata (complexity, target audience, source IDE)
- **Contained asset names, tags, and keywords** (for ranking — not returned as rows)

Does **not** return individual `rule.md`, `skill.md`, etc. as results.

### 5.3 Home and browse APIs

Idle Discover home loads section data from a dedicated endpoint — **not** search:

```http
GET /api/workflows/home
```

Returns `{ trending, recent }` — each an array of `WorkflowCard`.

Browse-all uses a separate paginated list endpoint:

```http
GET /api/workflows/browse?page=1&pageSize=12&types=rule,skill
```

`types` is optional (comma-separated component types). Returns `PagedResult<WorkflowCard>` sorted by downloads.

### 5.4 Search API (text query only)

Use search **only when the user submits a non-empty query**:

```http
POST /api/workflows/search
```

Returns `PagedResult<WorkflowCard>` — **workflows only**. Empty `query` is rejected (`400 SEARCH_QUERY_REQUIRED`).

Request contract (MVP):

```json
{
  "query": "pr review",
  "page": 1,
  "pageSize": 12,
  "sortBy": "relevance",
  "filters": {}
}
```

Type filtering during text search uses `filters.componentTypes` (workflows that **contain** those types).

### 5.5 URL state

`q` and `page` in route query params. Rebuild criteria from URL on every load. See
`ai-rules/cross-cutting/search-architecture-rules.md`.

### 5.6 Match highlight (optional on card)

When a query matched via a contained asset, the workflow card may show a subtle matched-asset line
(e.g. “matched: skill `code-review`”). The card still represents the **workflow**; click → detail.

### 5.7 Pagination

Default page size **12**. Prev/next on search and browse-all.

### 5.8 Empty states

- No workflows → friendly empty state.
- No search matches → broaden query + link to Agent Assets.

## 6. Discover home sections (no active query)

Loaded via `GET /api/workflows/home` — one request, not search.

| Section | MVP behaviour |
|---|---|
| **Trending workflows** | Highest `download_count` |
| **New workflows** | Recently published |
| **Featured workflows** | Post-MVP (curated) |
| **Categories** | Post-MVP — reserved in `filters` |
| **Recommended** | Post-MVP (personalized) |

## 7. Workflow card

Title, description, component-type badges + counts, tags, author, star count, download count, star toggle.
Optional matched-asset highlight on search results. Whole card → detail.

## 8. Engagement model

- **Stars:** binary toggle per user (`POST`/`DELETE /api/workflows/{id}/star`).
- **Downloads:** increment only when user clicks **Copy clone command** on detail (`POST /api/workflows/{id}/download`).
- No ratings/reviews in MVP. Install button is stubbed with an info toast only.

## Definition of done

- [ ] Idle home loads via `GET /api/workflows/home` (trending + new).
- [ ] Browse-all loads via `GET /api/workflows/browse` with optional type filters.
- [ ] Text search uses `POST /api/workflows/search` only with a non-empty `query`.
- [ ] Helper link to Agent Assets visible in search hero.
- [ ] URL-driven search state (`q`, `page`).
- [ ] No component/asset rows in Discover results.
- [ ] Empty / loading / error states handled.
