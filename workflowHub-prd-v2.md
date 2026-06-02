# Workflow Hub — MVP Product Requirements Document (PRD)

**Status:** Draft v0.4 (MVP scope) — GitHub link + clone command (no backend download, no IDE conversion)
**Last updated:** 31 May 2026
**Owner:** _(you)_

---

## 0. What this product is (updated)

Workflow Hub is a place to **publish and discover IDE workflows that live in public GitHub repos**, and to **get the complete workflow** (clone it at its pinned commit) on demand.

A **workflow is a bundle of one or more component types** — not just commands. A single workflow can contain any mix of these **five** component types: **rules**, **commands**, **subagents**, **hooks**, and **skills**. Each component is indexed and searchable **individually**, and discovery can be **filtered by component type** (see §4.3 and §4.5). Other files in the repo (scripts, configs, docs, MCP/plugin settings, `README`s) are **not indexed as components** — they are bundled and installed with the workflow but are not separately searchable/filterable.

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
- **Search** across all workflows at both the workflow level and the command/capability level, and be told *which workflow* contains a given thing.
- **Get** a workflow: we show the GitHub repo link + a `git clone` command pinned to the commit. No backend download, no IDE conversion.

### 1.3 Why pin the commit SHA
The version a user gets must be the version that was shown on the detail page. If we always referenced "latest," an author could push a change and a user would get something different from what they saw. Pinning the SHA at publish makes every clone reproducible.

---

## 2. Goals & Non-goals

### 2.1 MVP Goals
- Google-only sign-in, with our own backend-minted JWT.
- Publish a workflow as a pinned reference (repo URL + commit SHA + metadata + manifest). No permanent source copies.
- Search workflows by title/description **and** by component, returning which workflow + file contains the match.
- Browse of workflows — **login required** (see §4.5; the whole Discover area is behind auth).
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

> **Ratings note (decided):** ratings are **dropped for MVP**. Instead, each workflow shows an
> **install count** (`install_count`) on cards and the detail page. A full ratings/reviews system is
> a possible post-MVP fast-follow.

---

## 3. Users & key use cases

| Persona | Goal |
|---|---|
| **Author** | Publish a workflow from their public repo so others can discover and get it. |
| **Installer** | Find a workflow (or a specific component), then get the whole workflow (clone it). |

**Primary use cases**
1. *As an Author*, I log in with Google and publish a workflow by pasting a public repo URL; we pin the commit and index its files.
2. *As an Installer*, I search "github commit summary," open the result, and get the GitHub link + a pinned `git clone` command for the workflow.
3. *As an Installer*, I search for a specific component (e.g. a skill or hook) and the result tells me which workflow + file contains it.

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
- GEN-3: Taking a workflow (clicking get / copying the command) increments `install_count`.
- GEN-4: To use a workflow in a **different IDE**, we return a ready-to-run **conversion prompt** (the user runs it in their own agent). No server-side conversion.
- GEN-5: Clear, user-facing errors if the repo is gone/private or the commit no longer exists (see §7).
- GEN-6: Before the clone command, show the workflow's **MCP/plugin dependencies** panel (§4.6, DEP-4) so the user sets up required servers/plugins first.

### 4.5 Search
Served from the manifest/search index (§5.3):
- SR-1 **Workflow search:** matches title/description/tags → ranked workflows.
- SR-2 **Component search:** matches a component's author/derived `summary`/`capabilities`/`keywords` → returns the **component (file) + parent workflow** ("this hook appears in Workflow Y → file `on-save.sh`"). Covers all five types — rule, command, subagent, hook, skill.
- SR-3 **Filter by component type:** the user can constrain search/browse to one or more `component_type` values (e.g. only `rule`, only `subagent`, or `command`+`hook`). Each type is also browsable on its own.
- SR-4 **Per-type facets:** results show counts per component type so the user sees "12 rules, 4 commands, 2 subagents" and can drill into each individually.
- SR-5 Results link straight to the workflow (ideally the matching component/file).
- SR-6 **Login required:** browse, search, and filters are all behind auth — logged-out users are redirected to sign-in. (The whole Discover area requires login.)
- SR-7 Graceful empty-state handling (including "no results for this type filter").

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
  install_count (int, default 0)  # number of installs/downloads; shown on cards & detail (replaces ratings)
  visibility (enum: public)  # MVP: public only (but Discover/browse still requires login)
  search_vector (tsvector)   # generated from manifest + title + tags
  created_at, updated_at

# NOTE: no File table and no file content stored. Users clone from GitHub directly.
```

### 5.2 Storage strategy (the key decision)
- **No permanent source copies.** A workflow is a pinned reference + metadata + manifest only.
- **No backend download:** users clone from GitHub directly; we store no files and run no download jobs.
- **Profile avatars:** no upload/object storage. We reuse the Google account's `avatar_url` (captured at login from the Google ID token) as the profile picture — no file storage involved.
- **Snapshotting (v2):** copying repo files into our storage as a permanent backup is a *reliability* upgrade for later — not a day-one need (see §7).

### 5.3 Search architecture
- MVP: Postgres full-text search. Build `search_vector` from BOTH the author and derived manifest blocks (summaries, capabilities, keywords) + workflow title/tags; weight `derived` higher; GIN index; rank with `ts_rank`.
- Powers SR-1 (workflow) and SR-2 (component) from one index.
- **Component-type filtering (SR-3/SR-4):** index `component_type` per manifest entry and the denormalized `component_types[]` on the workflow. Queries can filter by one or more types and compute per-type facet counts. GIN index on `component_types[]` for fast filtering.
- Post-MVP: vector embeddings (pgvector) for natural-language/semantic queries, hybrid ranking. Not required for MVP.

---

## 6. API surface (illustrative)

```
POST   /auth/google         # Google ID token -> our JWT
POST   /auth/refresh
POST   /auth/logout
GET    /me                  # current user (protected)

POST   /workflows           # publish: validate repo+commit, fetch once, build manifest (author+derived) (protected)
GET    /workflows           # browse (protected — login required)
GET    /workflows/:id       # detail incl. manifest, repo, commit, install_count (protected)
PATCH  /workflows/:id       # edit metadata / re-publish (bump commit) (owner)
DELETE /workflows/:id       # delete (owner)

GET    /workflows/:id/get             # returns repo link + pinned `git clone` command; increments install_count (protected)
GET    /workflows/:id/convert-prompt?target_ide=...   # returns a prompt the user runs to convert to their IDE (protected)
GET    /publish/template              # download the analysis prompt + manifest template (protected)
GET    /search?q=...&scope=workflow|component&component_type=rule,command,subagent,hook,skill   # protected
       # scope: search whole workflows or individual components
       # component_type: optional CSV filter (one or more of the five); response includes per-type facet counts
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
3. ~~Ratings in MVP~~ — **decided: dropped for MVP; show `install_count` instead** (see §2.2).
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
- [ ] Logged-in users can browse and open workflows (browse/search are login-gated).
- [ ] A workflow can contain a mix of the five component types (rule, command, subagent, hook, skill); each is individually indexed.
- [ ] Workflow-level search works.
- [ ] Component search returns the component (file) + which workflow it's in, for **all five** component types (not commands only).
- [ ] Search/browse can be filtered by component type, with per-type facet counts.
- [ ] Workflow cards/detail show `install_count`; getting a workflow increments it.
- [ ] Getting a workflow shows its GitHub link + a `git clone` command pinned to the commit.
- [ ] Publish stores BOTH author-supplied and our derived manifest data per component.
- [ ] Publish scans for MCP/plugin config, pre-fills detected dependencies, and lets the author add/confirm them via a small form (kind, name, required/optional, short note — no secrets).
- [ ] Detail page shows the MCP/plugin dependency panel above the clone command.
- [ ] Author can generate manifest input via the provided analysis prompt + template.
- [ ] Workflow stores/shows its source IDE; a conversion prompt is offered for a different IDE.
- [ ] Clear error when source repo/commit is unavailable.