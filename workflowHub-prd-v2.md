# Workflow Hub — MVP Product Requirements Document (PRD)

**Status:** Draft v0.5 (MVP scope) — GitHub link + clone command (no backend download, no IDE conversion)
**Last updated:** 7 Jun 2026
**Owner:** _(you)_

---

## 0. What this product is (updated)

Workflow Hub is a place to **publish and discover IDE workflows that live in public GitHub repos**, and to **get the complete workflow** (clone it at its pinned commit) on demand.

A **workflow is a bundle of one or more agent asset types** — not just commands. A single workflow can contain any mix of these **five** indexed types: **rules**, **commands**, **subagents**, **hooks**, and **skills**. Other files in the repo (scripts, configs, docs, MCP/plugin settings, `README`s) are **not indexed** — they are bundled at install but are not searchable.

**Workflows and agent assets are different products.** Discover is a **Workflow Marketplace** (returns workflows only). **Agent Assets** is a separate **AI Agent Asset Marketplace** (returns individual assets only). Each has its own search endpoint, ranking, filters, and pagination (see §4.5, §4.7, §5.3). Workflow search may use contained asset names/tags for relevance, but individual assets never appear as Discover results.

Key shift from v0.1: **we do not store copies of workflow files.** A published workflow is essentially a *pinned reference* to a public GitHub repo — its URL, the exact commit SHA at publish time, plus searchable metadata. We never host or stream the files. To get a workflow, the user is given the GitHub repo link and a `git clone` command pinned to the exact commit. (We fetch the repo once at publish to build the manifest, then discard it — see §4.4 and §6.)

Each workflow is tagged with the **IDE it was built for** (e.g. Claude, Cursor, Copilot). To use it in a different IDE, we give the user a **conversion prompt** to run in their own agent — we don't convert on the server.

---

## 1. Overview

### 1.1 Problem
People build useful IDE workflows (rules, commands, agent configs, scripts) and keep them in GitHub repos, but there's no good way to **publish them for discovery** and let others **find and download them**. Today you have to know the repo exists and dig through it to understand what's inside.

### 1.2 Solution (MVP)
A web platform where users:
- Log in with **Google** (only auth method in MVP), with **our own JWT** minted by the backend.
- **Publish** a workflow by pointing at a public GitHub repo; we pin the commit SHA and store metadata + a small manifest. **No file copies.**
- **Discover workflows** in the Workflow Marketplace — search by outcome (“PR review workflow”), including matches inside contained assets for ranking, but results are **workflows only**.
- **Discover agent assets** in a separate Agent Asset Marketplace — search for individual rules, commands, skills, hooks, and agents.
- **Get** a workflow: we show the GitHub repo link + a `git clone` command pinned to the commit. No backend download, no IDE conversion.

### 1.3 Why pin the commit SHA
The version a user gets must be the version that was shown on the detail page. If we always referenced "latest," an author could push a change and a user would get something different from what they saw. Pinning the SHA at publish makes every clone reproducible.

---

## 2. Goals & Non-goals

### 2.1 MVP Goals
- Google-only sign-in, with our own backend-minted JWT.
- Publish a workflow as a pinned reference (repo URL + commit SHA + metadata + manifest). No permanent source copies.
- **Workflow search** (Discover): title/description/tags/author/metadata + contained asset text for relevance → ranked **workflows only**.
- **Agent asset search** (Agent Assets): asset name/description/content/tags/type → ranked **assets only**.
- Browse/search for both marketplaces — **login required** (see §4.5, §4.7).
- **Get the workflow:** show the GitHub repo link + a `git clone` command pinned to the commit. No backend download.

### 2.2 Non-goals (out of scope for MVP)
- Other login providers (GitHub OAuth, email/password, SSO).
- **Permanent snapshotting** of repo files into our own storage (this is the v2 reliability upgrade — see §7).
- Versioning / multiple published versions per workflow (MVP = one pinned commit per workflow; re-publish to update).
- Private repos / private workflows / team permissions (everything is public).
- Running workflows in-browser.
- Payments / monetization.
- Semantic / AI vector search (fast-follow; MVP uses keyword + structured fields — §5.3).
- Server-side conversion into a target IDE format (instead we tag the source IDE and give the user a conversion prompt to run themselves — see §4.4).
- Hosting, fetching, zipping, or streaming workflow files for download (users clone from GitHub directly).

> **Engagement note (decided):** ratings/reviews are **dropped for MVP**. Workflows show **star count**
> (binary toggle per user) and **download count** on cards and detail. Agent assets show **star count**
> only — no download counter. Download count increments only when the user clicks **Copy clone command**
> on workflow detail (not on page view).

---

## 3. Users & key use cases

| Persona | Goal |
|---|---|
| **Author** | Publish a workflow from their public repo so others can discover and get it. |
| **Installer** | Find a workflow (solution) or a reusable agent asset, then get the whole workflow (clone it). |
| **Builder** | Find a specific rule, command, skill, hook, or agent to reuse or inspect. |

**Primary use cases**
1. *As an Author*, I log in with Google and publish a workflow by pasting a public repo URL; we pin the commit and index its files.
2. *As an Installer*, I search Discover for "PR review workflow," open the result, and get the GitHub link + a pinned `git clone` command.
3. *As a Builder*, I search Agent Assets for "cursor rule code review," open the asset, and see which workflow contains it before installing the whole bundle.

---

## 4. MVP Feature Requirements

### 4.1 Authentication — Google login + our JWT

**Flow**
1. Frontend triggers Google Sign-In and obtains a Google **ID token**.
2. Frontend sends it to `POST /auth/google`.
3. Backend **verifies** the ID token against Google's public keys (signature, `aud`, `iss`, `exp`).
4. Backend **finds or creates** the user (keyed by Google `sub`).
5. Backend mints **our own JWT** — short-lived access token (~15 min) + longer-lived refresh token (~7–30 days).
6. Protected calls send `Authorization: Bearer <access_token>`; backend validates **our** JWT on every protected route.
7. `POST /auth/refresh` rotates the access token; `POST /auth/logout` invalidates the refresh token.

**Requirements**
- AUTH-1: Only Google login in MVP.
- AUTH-2: JWTs signed & verified by our backend (RS256 recommended; HS256 acceptable for MVP).
- AUTH-3: Short-lived access tokens + refresh flow.
- AUTH-4: Protected routes reject missing/expired/invalid tokens with `401`.
- AUTH-5: User record: `id`, `google_sub`, `email`, `name`, `avatar_url`, `created_at`.

### 4.2 Publish a workflow (reference, not copy)

At publish we store **almost nothing file-wise**:
- WF-1: Metadata — title, description, tags, author.
- WF-1b: Author-provided manifest input — per component: path, component_type (one of 5), a clear "what this file does" summary, capabilities, keywords. Author can fill this manually OR generate it with our analysis prompt+template (WF-10).
- WF-2: `repo_url` — the public GitHub repo.
- WF-3: `commit_sha` — pinned at the moment of publishing.
- WF-4: `manifest` — per-component index storing BOTH the author-provided data and our own server-side analysis (`author` + `derived` blocks; see §4.3). Built by fetching the repo **once** at publish, then discarding the files.
- WF-5: No permanent source file copies are kept.

**Publish-time processing**
- WF-6: On publish, fetch the repo at `commit_sha` once, walk the tree, run our analysis to build the `derived` manifest data, merge with the author's input, then delete the fetched files.
- WF-7: Validate the repo is public and the commit exists; reject otherwise with a clear error.
- WF-8: Owner can edit metadata/tags, **re-publish** to bump the pinned commit, or delete the workflow.
- WF-9: Any **logged-in** user can view a published workflow's detail page (browse is login-gated — see §4.5).
- WF-10: Provide a downloadable **analysis prompt + manifest template**. The author runs it in their own AI agent against their repo; it outputs manifest JSON in our schema, which they paste/upload at publish. (See the Publish module doc.)
- WF-11: Author selects the **source IDE** the workflow was built for (e.g. claude, cursor, copilot); stored and shown on the workflow.
- WF-12: At publish, the author declares the workflow's **MCP/plugin dependencies** (see §4.6). MCP/plugin config is usually user-level and **not present in the repo**, so we cannot rely on the repo alone — the author must confirm/complete the list. **No secrets are ever collected.**

### 4.3 File-level metadata (the search backbone)

Captured once at publish and stored in the manifest. Per file:
- `path` — location in the repo.
- `component_type` — **one of exactly five values** (the search/filter backbone):
  - `rule` — IDE rules / instruction files (e.g. cursor rules).
  - `command` — slash commands / invocable commands.
  - `subagent` — subagent / agent definitions.
  - `hook` — lifecycle hooks (pre/post events, automation triggers).
  - `skill` — skill definitions (e.g. Claude Code / Cursor skills).
  - Files that match none of the five (MCP/plugin configs, scripts, docs, `README`s) are **not
    indexed as components** — they're bundled at install but carry no `component_type` record.
- `language` — file language/format (e.g. md, json, yaml, sh, ts).
- `author` — author-supplied block: `summary`, `capabilities[]`, `keywords[]` (what each file does, in their words / via our template — WF-10).
- `derived` — our server-side analysis block: `summary`, `capabilities[]`, `keywords[]` (the trust anchor; weighted higher in search).
- `flags` — trust signals: `type_mismatch` (author vs derived disagree on type), `verified`.

> **Each component type is a first-class, individually indexed entity.** A workflow's manifest is a list of these component records; a single workflow may contain rules **and** commands **and** subagents **and** hooks **and** skills simultaneously.

Population: **author-supplied (via our prompt+template, WF-10) MERGED with our own server-side analysis.** Both blocks are stored; search uses both, display prefers `derived`. Neither source alone is trusted blindly — a mismatch between them is a moderation signal.

### 4.4 Get the workflow — GitHub link + clone command

No backend download. We show the GitHub repo link and a `git clone` command pinned to the commit:

```
git clone <repo_url>
git -C <repo> checkout <commit_sha>
```

**Requirements**
- GEN-1: Always reference the **pinned** `commit_sha`, never "latest."
- GEN-2: We do **not** host, fetch, zip, or stream the files — the user clones from GitHub directly.
- GEN-3: Clicking **Copy clone command** on workflow detail increments `download_count` (each click counts once).
- GEN-4: To use a workflow in a **different IDE**, we return a ready-to-run **conversion prompt** (the user runs it in their own agent). No server-side conversion.
- GEN-5: Clear, user-facing errors if the repo is gone/private or the commit no longer exists (see §7).
- GEN-6: Before the clone command, show the workflow's **MCP/plugin dependencies** panel (§4.6, DEP-4) so the user sets up required servers/plugins first.

### 4.5 Discover — Workflow Marketplace search

Discover helps users find **complete solutions**. User intent: *"I want a workflow that does X."*

**Search scope (ranking inputs):** workflow title, description, tags, categories (future), author, metadata, and **contained asset names/tags/keywords** (for relevance only).

**Returns:** **workflows only** — never individual asset files as top-level rows.

**API:** `GET /api/workflows/home` for idle browse sections; `GET /api/workflows/browse` for paginated type-filter browse; `POST /api/workflows/search` when the user enters a text query (`query`, `page`, `pageSize`, `sortBy`, `filters`).

**Requirements**
- SR-1 **Workflow-only results:** every row is a workflow card; assets may influence ranking and optional on-card match highlight, but not appear as separate result types.
- SR-2 **Contained asset matching:** searching "cursor" or "pr review" can surface a workflow because an contained asset matches — user still gets the workflow.
- SR-3 **Discover sections (idle browse):** Trending (by downloads), New (recent), Featured (curated — post-MVP), Categories (post-MVP).
- SR-4 **Browse-all:** paginated workflow list; optional `filters.componentTypes` = workflows that contain selected types.
- SR-5 **Login required:** Discover browse/search behind auth.
- SR-6 **URL-driven search state** on the frontend; server-side pagination from day one.
- SR-7 Graceful empty states + link to Agent Assets ("Looking for rules, commands, skills…?").

### 4.7 Agent Assets — AI Agent Asset Marketplace search

Agent Assets helps **builders** find reusable AI building blocks. User intent: *"I need a Cursor rule / Claude command / skill / hook / agent."*

**Search scope:** asset name, description, content keywords, tags, author, asset type.

**Returns:** **individual agent assets only** — not workflows as top-level rows (parent workflow shown as secondary context on the card).

**Asset types (MVP):** `rule`, `command`, `skill`, `hook`, `subagent` (displayed as Agents). Future: templates, prompts, subskills — extend types without UX redesign.

**API:** `GET /api/agent-assets/home` for idle browse sections; `GET /api/agent-assets/browse` for paginated type-filter browse; `POST /api/agent-assets/search` when the user enters a text query — same structured request shape as workflow search.

**Requirements**
- AA-1 **Asset-only results:** each row is one indexed manifest component (`WorkflowComponent`).
- AA-2 **Type filter chips:** All, Rules, Commands, Skills, Hooks, Agents — via `filters.assetTypes`.
- AA-3 **Sections (idle browse):** Popular (by asset star count), New (recently published).
- AA-4 **Categories:** post-MVP — reserved in `filters`.
- AA-5 **Login required**; URL-driven search state; paginated responses.
- AA-6 Clicking an asset opens the parent workflow detail (highlight asset).

### 4.8 Learn — knowledge center

Learn is WorkflowHub's **knowledge center**. Its purpose is to educate users about workflows, agent assets, design principles, publishing standards, best practices, and platform concepts.

**Not a traditional blog** — it mixes documentation, guides, knowledge-base articles, and educational content.

**Content types**
- **Guide** — long-lived, foundational, may be pinned (3–5 max). Examples: *How to Create High Quality Workflows*, *Understanding Workflows*, *Understanding Agent Assets*, *Publishing Guidelines*.
- **Article** — educational content published over time. Examples: *Building a Research Workflow*, *Common Workflow Mistakes*.

**Search scope:** article title, summary, markdown content (via denormalized search text), category name.

**Returns:** **articles only** — never workflows or agent assets. Learn search is fully isolated from Discover and Agent Assets.

**API:** `POST /api/articles/search` — structured request (`query`, `page`, `pageSize`, optional `filters.category`).

**Homepage sections (idle browse):** Getting Started (pinned guides), Latest Articles, Categories.

**URLs:** `/learn`, `/learn/:slug` (e.g. `/learn/how-to-create-high-quality-workflows`).

**Content format:** article body stored as **Markdown** from day one.

**Requirements**
- LN-1 **Article-only results:** Learn search never returns workflows or agent assets.
- LN-2 **Pinned guides:** 3–5 pinned onboarding guides on the Learn homepage.
- LN-3 **Categories:** Workflow Design, Agent Assets, Architecture, Tutorials, Best Practices (MVP seed).
- LN-4 **Login required** for Learn browse/search (consistent with other marketplaces).
- LN-5 **URL-driven search state** on the frontend; server-side pagination.
- LN-6 **Super Admin only** creates/manages content in MVP (seed + API; admin UI post-MVP).
- LN-7 Article detail renders markdown; guides and articles share the same detail UX with a type badge.

**Roles (MVP)**
- **User:** read, search, share articles.
- **Super Admin:** create, edit, delete, publish, pin articles; manage categories.

Future roles (Contributor, Editor, Moderator) are post-MVP.

### 4.6 Workflow dependencies (MCP servers & plugins)

MCP servers and plugins are **not** one of the five component types — they are external
**dependencies** the workflow needs in order to run. Because their config usually lives at the
**user level** (`~/.cursor/mcp.json`, `~/.claude.json`) and not in the repo, a downloader who just
clones the repo won't get them. So we **declare** them at publish and **disclose** them before install.

**At publish (author side)**
- DEP-1: On publish we **scan the repo** for MCP/plugin config (`.mcp.json`, `.cursor/mcp.json`,
  `.vscode/mcp.json`, `.claude-plugin/`) and **pre-fill** any servers/plugins we can identify.
- DEP-2: The author confirms the detected list and can **add entries the repo doesn't include**
  (the user-level ones) via a small form. Per dependency:
  - `kind` — `mcp` | `plugin`
  - `name` — e.g. `github`, `postgres`, `anthropics/web-tools`
  - `requirement` — `required` | `optional`
  - `note` — short, optional one-liner (e.g. "needs a local Postgres"). **No secrets, no env values, no tokens.**
- DEP-3: A clear **"runs standalone / no dependencies"** option so simple workflows skip this.

**At install (downloader side)**
- DEP-4: The workflow detail page shows a **"Before you install — you'll need:"** panel listing each
  MCP server / plugin, whether it's `required` or `optional`, and the short note — rendered **above**
  the clone command so users set them up first.
- DEP-5: We never store or display secrets — only the dependency **name**, requirement, and note.

---

## 5. Data, storage & search

### 5.1 Data model (MVP)

```
User
  id (uuid, pk)
  google_sub (string, unique)
  email (string, unique)
  name, avatar_url
  created_at

Workflow
  id (uuid, pk)
  owner_id (fk -> User)
  title, description
  tags (string[])
  repo_url (string)
  commit_sha (string)        # pinned at publish
  source_ide (string)        # IDE the workflow targets: claude | cursor | copilot | ...
  manifest (jsonb)           # [{path, component_type, language,
                             #    author:{summary, capabilities, keywords},     # what the author gave
                             #    derived:{summary, capabilities, keywords},    # our own analysis
                             #    flags:{type_mismatch, verified}}, ...]
                             # component_type in: rule | command | subagent | hook | skill   (exactly these five)
                             # files matching none of the five are NOT recorded as components
  component_types (string[]) # distinct component_types present (denormalized for fast filtering/facets)
  dependencies (jsonb)       # MCP/plugin deps (NOT components, NOT secrets) — §4.6
                             # [{kind: mcp|plugin, name, requirement: required|optional, note}, ...]
  star_count (int, default 0)       # denormalized; updated by workflow_stars
  download_count (int, default 0)   # copy-clone clicks; shown on cards & detail
  visibility (enum: public)  # MVP: public only (but Discover/browse still requires login)
  search_vector (tsvector)   # generated from manifest + title + tags
  created_at, updated_at

# NOTE: no File table and no file content stored. Users clone from GitHub directly.

ArticleCategory
  id (uuid, pk)
  name, slug (unique), description

Article
  id (uuid, pk)
  slug (string, unique)
  title, summary
  content (text)           # markdown body
  category_id (fk -> ArticleCategory)
  author_id (fk -> User)
  status (enum: draft | published)
  content_type (enum: guide | article)
  is_pinned (bool)
  published_at
  search_text (text)       # denormalized for keyword search
  created_at, updated_at
  + soft-delete / audit fields (ITrackable)
```

### 5.2 Storage strategy (the key decision)
- **No permanent source copies.** A workflow is a pinned reference + metadata + manifest only.
- **No backend download:** users clone from GitHub directly; we store no files and run no download jobs.
- **Profile avatars:** no upload/object storage. We reuse the Google account's `avatar_url` (captured at login from the Google ID token) as the profile picture — no file storage involved.
- **Snapshotting (v2):** copying repo files into our storage as a permanent backup is a *reliability* upgrade for later — not a day-one need (see §7).

### 5.3 Search architecture

**Three independent search systems** — no mixed-entity endpoints. Each marketplace also exposes **dedicated home and browse APIs** so page loads never call search with an empty query.

| Marketplace | Home (idle sections) | Browse (filters, no text) | Search (text query required) | Returns |
|---|---|---|---|---|
| Discover (workflows) | `GET /api/workflows/home` | `GET /api/workflows/browse` | `POST /api/workflows/search` | Workflows only |
| Agent Assets | `GET /api/agent-assets/home` | `GET /api/agent-assets/browse` | `POST /api/agent-assets/search` | Agent assets only |
| Learn | `GET /api/articles/home` | — | `POST /api/articles/search` | Articles only |

Shared rules (see `ai-rules/cross-cutting/search-architecture-rules.md`):

- **Search** endpoints require a non-empty `query` and use structured request: `query`, `page`, `pageSize`, `sortBy`, `filters`.
- **Home** endpoints return curated section lists (trending/new, popular/new, pinned guides, etc.) — one call per page open.
- **Browse** endpoints paginate default lists with optional type filters — no full-text matching.
- Stateless server; ranking and pagination on the server; `PagedResult<T>` for browse/search.
- Frontend URL encodes search/filter state; criteria rebuilt from URL before each API call.

**Indexing (MVP):** Postgres `SearchText` on `Workflow` and `WorkflowComponent`, built at publish from author + derived manifest blocks. Workflow `SearchText` includes contained asset fields for Discover relevance. Component rows power Agent Asset search directly.

**Filtering:** `filters.componentTypes` on workflow search (workflows containing types). `filters.assetTypes` on asset search. Categories/post-MVP facets reserved in `filters`.

Post-MVP: vector embeddings (pgvector), hybrid ranking, featured/curated lists.

---

## 6. API surface (illustrative)

```
POST   /auth/google         # Google ID token -> our JWT
POST   /auth/refresh
POST   /auth/logout
GET    /me                  # current user (protected)

POST   /workflows           # publish (protected)
GET    /workflows/home      # discover home sections → DiscoverHomeDto (protected)
GET    /workflows/browse    # paginated browse + optional type filter → PagedResult<WorkflowCard> (protected)
POST   /workflows/search    # text search only (non-empty query) → PagedResult<WorkflowCard> (protected)
GET    /workflows/:id       # detail incl. manifest, repo, commit, star_count, download_count, is_starred (protected)
POST   /workflows/:id/star  # star workflow (protected)
DELETE /workflows/:id/star  # unstar workflow (protected)
POST   /workflows/:id/download  # increment download_count after copy-clone (protected)
PATCH  /workflows/:id       # edit metadata / re-publish (owner)
DELETE /workflows/:id       # delete (owner)

GET    /agent-assets/home   # popular + new sections → AgentAssetsHomeDto (protected)
GET    /agent-assets/browse # paginated browse + optional type filter → PagedResult<AgentAssetCard> (protected)
POST   /agent-assets/search # text search only (non-empty query) → PagedResult<AgentAssetCard> (protected)
POST   /agent-assets/:id/star   # star agent asset (protected)
DELETE /agent-assets/:id/star   # unstar agent asset (protected)
GET    /workflows/:id/convert-prompt?target_ide=...   # conversion prompt (protected)
GET    /publish/template              # analysis prompt + manifest template (protected)

GET    /articles/home                 # pinned guides, latest articles, categories (protected)
POST   /articles/search               # learn search → PagedResult<ArticleCard> (protected)
GET    /articles/:slug                # article detail incl. markdown content (protected)
```

---

## 7. Risks & the honest caveat
- **Fetch-based weakness:** if the author deletes the repo, makes it private, or force-pushes away the pinned commit, future installs of that workflow break.
- **MVP stance:** acceptable risk — most public repos stay up. Handle it with a clear error ("source repo or commit is no longer available").
- **When it starts mattering** (popular workflows, paying users): add **snapshotting** — at publish, copy repo files into object storage as a permanent backup and fetch from there. Same flow, different source. This is a **v2** upgrade, not day-one.

---

## 8. Non-functional requirements
- NFR-1: All protected endpoints enforce our JWT.
- NFR-2: Search returns in < ~500 ms on MVP dataset.
- NFR-3: Secrets (JWT key, Google client secret) out of source control.
- NFR-4: Temp job dirs always cleaned up, including on failure.
- NFR-5: HTTPS everywhere; refresh token in httpOnly cookie if used.
- NFR-6: Reasonable repo/zip size limits; reject pathological repos gracefully.

---

## 9. Open questions
1. ~~Target IDE(s) for MVP~~ — **decided: no IDE conversion; download the repo as-is** (see §4.4).
2. ~~Where do mapping rules live~~ — **N/A; no mapping in MVP.**
3. ~~Ratings in MVP~~ — **decided: dropped for MVP; stars + download_count instead** (see §2.2).
4. ~~Output zip cache~~ — **decided: no backend download at all; users clone from GitHub** (see §4.4).
5. ~~Sync vs queued worker~~ — **decided: N/A; no download job** (see §4.4).
6. Refresh token storage: httpOnly cookie vs client-held?
7. Repo/zip size limits?
8. Author-vs-derived manifest: how do we surface/flag mismatches in the UI?
9. What exactly does our server-side analysis run (LLM? heuristic parser?), and its cost ceiling?

---

## 10. MVP definition of done
- [ ] Google login works end-to-end; backend mints & validates our JWT.
- [ ] Author can publish a workflow from a public repo; commit SHA pinned; manifest built; no file copies kept.
- [ ] Owner can edit metadata / re-publish / delete.
- [ ] Logged-in users can browse Discover (workflows) and Agent Assets as separate experiences.
- [ ] A workflow can contain a mix of the five asset types; each is indexed at publish.
- [ ] Discover home loads via `GET /workflows/home`; browse-all via `GET /workflows/browse`; text search via `POST /workflows/search` (non-empty query only).
- [ ] `POST /workflows/search` returns workflows only; contained assets boost relevance but never appear as separate rows.
- [ ] Agent Assets home loads via `GET /agent-assets/home`; type-only browse via `GET /agent-assets/browse`; text search via `POST /agent-assets/search` (non-empty query only).
- [ ] `POST /agent-assets/search` returns individual assets only.
- [ ] Discover idle sections: trending + new workflows; Agent Assets: popular + new assets (from home APIs, not search).
- [ ] Type filter chips on Agent Assets; browse-all type filter on Discover.
- [ ] Workflow cards/detail show star + download counts; copy-clone increments download count.
- [ ] Getting a workflow shows its GitHub link + a `git clone` command pinned to the commit.
- [ ] Publish stores BOTH author-supplied and our derived manifest data per component.
- [ ] Publish scans for MCP/plugin config, pre-fills detected dependencies, and lets the author add/confirm them via a small form (kind, name, required/optional, short note — no secrets).
- [ ] Detail page shows the MCP/plugin dependency panel above the clone command.
- [ ] Author can generate manifest input via the provided analysis prompt + template.
- [ ] Workflow stores/shows its source IDE; a conversion prompt is offered for a different IDE.
- [ ] Clear error when source repo/commit is unavailable.
- [ ] Learn homepage shows pinned guides, latest articles, and categories.
- [ ] `POST /articles/search` returns articles only; isolated from workflow/asset search.
- [ ] Article detail pages render markdown content at `/learn/:slug`.
- [ ] Four pinned onboarding guides seeded (create workflows, understanding workflows, understanding agent assets, publishing guidelines).