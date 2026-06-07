# Agent Assets Module

**Status:** Decisions landing · **Last updated:** 6 Jun 2026
**Related PRD:** Agent Asset marketplace (§4.7), component model (§4.3), search architecture (§5.3).

---

## 1. What it is

The **Agent Assets** module is the **AI Agent Asset Marketplace** — a builder-focused discovery
experience for reusable agent building blocks: rules, commands, skills, hooks, subagents, and future
types (templates, prompts, …).

A workflow and an agent asset are **different products**. This module returns **individual assets
only** — never whole workflows as top-level search rows.

## 2. Access — LOGIN REQUIRED

Same as Discover: browse, search, and filters are behind auth.

## 3. Placement

```
App sidebar
  ├── Discover          ← Workflow Marketplace (outcomes / solutions)
  ├── Agent Assets      ← this module (reusable AI building blocks)
  ├── My workflows
  └── …
```

## 4. Screens

| Screen | Purpose |
|---|---|
| **Agent Assets home** | Search bar, type filter chips, popular/new asset lists, paginated asset results. |
| **Workflow detail** | Shared with Discover — opened when user follows parent-workflow link on an asset card. |

## 5. Search & filters

### 5.1 Search bar

Placeholder: `Search agent assets…`

Search scope (server-side):

- Asset name (title)
- Asset description (summary / capabilities)
- Asset content keywords / search phrases
- Asset tags (keywords)
- Asset author
- Asset type

### 5.2 Home API (idle sections)

```http
GET /api/agent-assets/home
```

Returns `{ popular, recent }` — each an array of `AgentAssetCard`. One call when the page opens with no query or type filters.

### 5.3 Browse API (type filters, no text query)

When the user selects type chips without searching:

```http
GET /api/agent-assets/browse?page=1&pageSize=12&types=rule,skill
```

`types` is optional. Returns `PagedResult<AgentAssetCard>` sorted by stars.

### 5.4 Search API (text query only)

Use search **only when the user submits a non-empty query**:

```http
POST /api/agent-assets/search
```

Returns `PagedResult<AgentAssetCard>` — **assets only**. Empty `query` is rejected (`400 SEARCH_QUERY_REQUIRED`).

Request contract (MVP):

```json
{
  "query": "pr review",
  "page": 1,
  "pageSize": 12,
  "sortBy": "relevance",
  "filters": {
    "assetTypes": "rule,skill"
  }
}
```

### 5.5 Asset type filter chips

Multi-select OR filter below the search bar:

```
All · Rules · Commands · Skills · Hooks · Agents
```

Maps to stored types: `rule`, `command`, `skill`, `hook`, `subagent` (displayed as **Agents**).

Future types (`template`, `prompt`, …) extend `filters.assetTypes` without UX redesign.

### 5.6 URL state

Search state lives in query params (`q`, `page`, `types`) — refresh and deep links restore identical
results. See `ai-rules/cross-cutting/search-architecture-rules.md`.

### 5.7 Pagination

Numbered pages / prev-next. Default page size **12**.

### 5.8 Empty states

- No assets published → friendly empty state.
- No results for query/filters → clear-filters affordance.

## 6. Asset card — proposed contents

```
┌──────────────────────────────────────────────────────────┐
│  [Rule]  code-review-rule.md                             │
│  One-line capability / summary…                          │
│  ↳ In workflow: PR Review Agent                          │
│  👤 Author · avatar                                      │
└──────────────────────────────────────────────────────────┘
```

Click → workflow detail, scrolled to / highlighting the asset.

## 7. Page sections (idle / no query)

Loaded via `GET /api/agent-assets/home` — one request, not search.

| Section | MVP behaviour |
|---|---|
| **Popular assets** | Sorted by asset `star_count` |
| **New assets** | Recently published (workflow `updated_at`) |
| **Categories** | Post-MVP — reserved in `filters` |
| **Asset types** | Filter chips (§5.3) |

## 8. Relationship to Discover

| Discover (Workflow Marketplace) | Agent Assets |
|---|---|
| User intent: “I need a workflow that does X” | User intent: “I need a Cursor rule / Claude command / skill” |
| Returns workflows only | Returns assets only |
| May rank using contained asset text | Ranks asset fields directly |
| `GET /workflows/home` + `POST /workflows/search` | `GET /agent-assets/home` + `GET /agent-assets/browse` + `POST /agent-assets/search` |

Assets indexed at publish time (`WorkflowComponent` rows) power this module. They are **not** mixed
into Discover search results.

## Definition of done

- [ ] Agent Assets nav item routes to `/workflows/agent-assets`.
- [ ] Idle home loads via `GET /api/agent-assets/home`.
- [ ] Type-only filtering uses `GET /api/agent-assets/browse`.
- [ ] Text search uses `POST /api/agent-assets/search` only with a non-empty `query`.
- [ ] Results are asset cards only; parent workflow shown as secondary context.
- [ ] Type filter chips + URL-driven state work end-to-end.
- [ ] Empty / loading / error states handled.
