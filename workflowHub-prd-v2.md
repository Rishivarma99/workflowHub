# Workflow Hub — MVP Product Requirements Document (PRD)

**Status:** Draft v0.2 (MVP scope) — fetch-based generation model
**Last updated:** 30 May 2026
**Owner:** _(you)_

---

## 0. What this product is (updated)

Workflow Hub is a place to **publish and discover IDE workflows that live in public GitHub repos**, and to **install/convert them into a target IDE format on demand** (e.g. Cursor).

Key shift from v0.1: **we do not store copies of workflow files.** A published workflow is essentially a *pinned reference* to a public GitHub repo — its URL, the exact commit SHA at publish time, plus searchable metadata. The actual files are fetched from GitHub only at install time, transformed, zipped, streamed to the user, and deleted. (See §4.4 and §6.)

---

## 1. Overview

### 1.1 Problem
People build useful IDE workflows (rules, commands, agent configs, scripts) and keep them in GitHub repos, but there's no good way to **publish them for discovery** and let others **install them into their own IDE** in the right format. Today you have to find the repo, understand its layout, and manually reshape it for your tool.

### 1.2 Solution (MVP)
A web platform where users:
- Log in with **Google** (only auth method in MVP), with **our own JWT** minted by the backend.
- **Publish** a workflow by pointing at a public GitHub repo; we pin the commit SHA and store metadata + a small manifest. **No file copies.**
- **Search** across all workflows at both the workflow level and the command/capability level, and be told *which workflow* contains a given thing.
- **Install/convert** a workflow into a chosen target IDE: we fetch the repo at the pinned commit, apply a mapping, zip it, and stream it down.

### 1.3 Why pin the commit SHA
The version a user installs must be the version that was shown (and rated) on the detail page. If we always fetched "latest," an author could push a change and a user would install something different from what they saw. Pinning the SHA at publish makes every install reproducible.

---

## 2. Goals & Non-goals

### 2.1 MVP Goals
- Google-only sign-in, with our own backend-minted JWT.
- Publish a workflow as a pinned reference (repo URL + commit SHA + metadata + manifest). No permanent source copies.
- Search workflows by title/description **and** by command/capability, returning which workflow + file contains the match.
- Public browse of workflows.
- **Fetch-based install/convert:** given a workflow + target IDE, fetch at the pinned commit → map → zip → stream → delete.

### 2.2 Non-goals (out of scope for MVP)
- Other login providers (GitHub OAuth, email/password, SSO).
- **Permanent snapshotting** of repo files into our own storage (this is the v2 reliability upgrade — see §7).
- Versioning / multiple published versions per workflow (MVP = one pinned commit per workflow; re-publish to update).
- Private repos / private workflows / team permissions (everything is public).
- Running workflows in-browser.
- Payments / monetization.
- Semantic / AI vector search (fast-follow; MVP uses keyword + structured fields — §5.3).

> **Ratings note:** ratings were referenced as detail-page metadata. MVP can ship with rating *fields* on the workflow record (avg + count) and optionally a 1-tap rating UI, but a full ratings/reviews system is a fast-follow. Flagged as an open decision in §9.

---

## 3. Users & key use cases

| Persona | Goal |
|---|---|
| **Author** | Publish a workflow from their public repo so others can discover and install it. |
| **Installer** | Find a workflow (or a specific command), then install/convert it into their IDE. |

**Primary use cases**
1. *As an Author*, I log in with Google and publish a workflow by pasting a public repo URL; we pin the commit and index its files.
2. *As an Installer*, I search "github commit summary," open the result, pick my IDE (e.g. Cursor), and download a ready-to-use zip.
3. *As an Installer*, I search for a specific command and the result tells me which workflow + file contains it.

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
- WF-2: `repo_url` — the public GitHub repo.
- WF-3: `commit_sha` — pinned at the moment of publishing.
- WF-4: `manifest` — a small index of *which file is which type* (+ target-mapping hints + per-file search descriptions). Built by fetching the repo **once** at publish, then discarding the files.
- WF-5: No permanent source file copies are kept.

**Publish-time processing**
- WF-6: On publish, fetch the repo at `commit_sha` once, walk the tree, build the manifest and the per-file search descriptions (§4.3), then delete the fetched files.
- WF-7: Validate the repo is public and the commit exists; reject otherwise with a clear error.
- WF-8: Owner can edit metadata/tags, **re-publish** to bump the pinned commit, or delete the workflow.
- WF-9: Anyone can view a published workflow's detail page (public browse).

### 4.3 File-level metadata (the search backbone)

Captured once at publish and stored in the manifest. Per file:
- `path` — location in the repo.
- `type` / `language` — e.g. cursor-rule, command, config, script.
- `summary` — one-line plain-language description of what the file does.
- `commands` — notable commands/capabilities (name + short description).
- `keywords` — extracted or author-provided terms.
- `target_mapping` — hint for where/how this file maps in a target IDE (used at install).

Population: **manual with an optional auto-draft** (parser/LLM generates a draft summary/commands at publish; author can edit). Ships fast, keeps search quality from depending solely on author diligence.

### 4.4 Install / Convert — the fetch-based generation flow

Input: `(repo_url, commit_sha, target_ide)` → Output: a downloadable zip.

```
1. CACHE CHECK
   key = hash(repo_url, commit_sha, target_ide)
   if object storage has a zip for this key  ->  stream it, done.

2. FETCH (no permanent copy)
   mkdir /tmp/job-<id>
   download tarball of the EXACT commit:
     https://github.com/<owner>/<repo>/archive/<commit_sha>.tar.gz
   extract into /tmp/job-<id>

3. MAP
   load mapping rules for target_ide
   apply per manifest: move/rename folders, light frontmatter edits

4. ZIP
   zip the transformed tree

5. (OPTIONAL) CACHE OUTPUT
   upload the zip to object storage under `key`, with a TTL

6. STREAM
   stream the zip to the user

7. CLEANUP (always, even on error)
   rm -rf /tmp/job-<id>
```

**Requirements**
- GEN-1: Always fetch the **pinned** `commit_sha`, never "latest."
- GEN-2: Source files exist only transiently in `/tmp/job-<id>` and are deleted right after zipping (and on failure).
- GEN-3: Output zip cache (object storage, keyed by `repo+commit+target_ide`, with TTL e.g. 30 days) is a **speed optimization** — optional for v1, can be skipped.
- GEN-4: Conversion runs in a backend worker; long jobs shouldn't block the request thread (queue/worker acceptable for MVP if needed).
- GEN-5: Clear, user-facing errors if the repo is gone/private or the commit no longer exists (see §7).

### 4.5 Search
Served from the manifest/search index (§5.3):
- SR-1 **Workflow search:** matches title/description/tags → ranked workflows.
- SR-2 **Command/capability search:** matches a file's `summary`/`commands`/`keywords` → returns the **file + parent workflow** ("this command appears in Workflow Y → file `deploy.sh`").
- SR-3 Results link straight to the workflow (ideally the matching file).
- SR-4 Search available to logged-out users.
- SR-5 Graceful empty-state handling.

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
  manifest (jsonb)           # [{path, type, summary, commands, keywords, target_mapping}, ...]
  avg_rating (float, null)   # optional / fast-follow
  ratings_count (int, default 0)
  visibility (enum: public)  # MVP: public only
  search_vector (tsvector)   # generated from manifest + title + tags
  created_at, updated_at

# NOTE: no File table and no file content stored. Files are fetched on demand.
```

### 5.2 Storage strategy (the key decision)
- **No permanent source copies.** A workflow is a pinned reference + metadata + manifest only.
- **During a job:** temp files in the worker's `/tmp/job-<id>`, deleted after zipping.
- **Optional output cache:** finished **zips** (never source files) in object storage (S3 / Cloudflare R2), keyed by `repo + commit + target_ide`, with a TTL. Speed optimization, not required for v1.
- **Snapshotting (v2):** copying repo files into our storage as a permanent backup is a *reliability* upgrade for later — not a day-one need (see §7).

### 5.3 Search architecture
- MVP: Postgres full-text search. Build `search_vector` from the manifest (summaries, command names/descriptions, keywords) + workflow title/tags; GIN index; rank with `ts_rank`.
- Powers both SR-1 (workflow) and SR-2 (command) from one index.
- Post-MVP: vector embeddings (pgvector) for natural-language/semantic queries, hybrid ranking. Not required for MVP.

---

## 6. API surface (illustrative)

```
POST   /auth/google         # Google ID token -> our JWT
POST   /auth/refresh
POST   /auth/logout
GET    /me                  # current user (protected)

POST   /workflows           # publish: validate repo+commit, fetch once, build manifest (protected)
GET    /workflows           # browse (public)
GET    /workflows/:id       # detail incl. manifest, repo, commit, rating (public)
PATCH  /workflows/:id       # edit metadata / re-publish (bump commit) (owner)
DELETE /workflows/:id       # delete (owner)

POST   /workflows/:id/install   # body: { target_ide } -> fetch-map-zip-stream (public)
GET    /search?q=...&type=workflow|command   # public
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
1. Confirm target IDE(s) for MVP — Cursor only, or more from day one?
2. Where do mapping rules live (config per target IDE)? Who maintains them?
3. Ratings in MVP: just display fields, a 1-tap rating, or defer entirely?
4. Output zip cache in v1 or skip until needed?
5. Sync vs queued worker for the install job?
6. Refresh token storage: httpOnly cookie vs client-held?
7. Repo/zip size limits?

---

## 10. MVP definition of done
- [ ] Google login works end-to-end; backend mints & validates our JWT.
- [ ] Author can publish a workflow from a public repo; commit SHA pinned; manifest built; no file copies kept.
- [ ] Owner can edit metadata / re-publish / delete.
- [ ] Anyone can browse and open workflows.
- [ ] Workflow-level search works.
- [ ] Command/capability search returns file + which workflow it's in.
- [ ] Install/convert: fetch at pinned commit → map → zip → stream → temp deleted.
- [ ] Clear error when source repo/commit is unavailable.