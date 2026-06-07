# Publish Module

**Status:** Frontend wizard + backend publish API built · **Last updated:** 6 Jun 2026
**Related PRD:** publish flow (§4.2), component model (§0, §4.3), search indexing (§4.5).
**Feeds:** [Discover module](discover-module.md) — component metadata powers search.

---

## 1. What it is

The **Publish** module lets an author register a public GitHub workflow repo as a searchable
Workflow Hub entry. We store a **pinned reference** (repo URL + commit SHA + metadata + component
records) — **never copies of workflow files**.

```
GitHub Repository  +  AI Analysis Output
        ↓
Workflow Record  +  Component Records  +  Searchable Metadata
```

**MVP philosophy:** no backend repository analysis. The user's AI agent analyzes the repo; the
backend validates, resolves GitHub links, indexes search data, and persists the workflow.

| Layer | Responsibility |
|---|---|
| **User** | Repo URL, source IDE, pasted AI JSON, final metadata review |
| **AI agent** | Workflow + component understanding, search metadata |
| **Backend** | Validation, commit SHA, GitHub URLs, name uniqueness, storage, indexing |

---

## 2. Access

**Login required.** Only authenticated users can publish.

---

## 3. Wizard — four steps

### Step 1 — Repository information

**User enters:** GitHub repository URL, source IDE (e.g. Cursor).

**On Next:** backend starts **non-blocking** background validation:

- Repository exists
- Repository is public
- Repository is accessible

Failures surface as inline status — they do **not** block advancing to Step 2.

---

### Step 2 — Repository analysis

**User flow:**

1. Copy the [repository analysis prompt](../prompts/repository-analysis-prompt.md)
2. Run it against the repository in their AI agent
3. Paste the generated JSON

**Backend validates:**

- Valid JSON
- Components present
- Valid component types (`rule`, `command`, `hook`, `skill`, `subagent`)
- Required fields present

No metadata editing in this step. Invalid JSON → show errors; user fixes paste and retries.

---

### Step 3 — Metadata & dependency review

Everything prefilled from AI output. User reviews and edits.

#### Workflow metadata

| Field | Notes |
|---|---|
| **Workflow name** | Validated for **uniqueness** here. Conflict → "Workflow name already exists"; user edits manually or picks a suggested alternative. No need to rerun AI. |
| **Description** | 1–3 sentences |
| **Tags** | 5–10 tags |

**[Use AI Suggested Name]** — restores the AI-generated name.

#### Dependencies

AI pre-fills MCP servers and plugins. User can add/remove entries and mark each **required** or
**optional**. Shown to installers before they install (Discover detail).

#### Setup complexity

Single select: **Beginner** · **Intermediate** · **Advanced**

#### Target audience

Single select: **Frontend** · **Backend** · **Full Stack** · **DevOps** · **AI Engineering** · **General**

#### Ownership confirmation

Required checkbox: *"I own this repository or have permission to publish this workflow."*

---

### Step 4 — Review & publish

Read-only summary:

- Repository URL, source IDE
- Workflow name, description, tags
- Component counts (by type)
- Dependencies, complexity, target audience

**Actions:** Back · **Publish**

On publish: resolve commit SHA, generate GitHub file links, persist workflow + components, index
for search.

---

## 4. Analysis prompt & expected JSON

The full prompt lives in **[`docs/prompts/repository-analysis-prompt.md`](../prompts/repository-analysis-prompt.md)**.
The Publish UI exposes a copy button; `{{REPOSITORY_URL}}` is substituted from Step 1.

**Top-level shape:**

```json
{
  "workflowName": "",
  "description": "",
  "tags": [],
  "suggestedDependencies": [],
  "components": [ /* see §5 */ ]
}
```

Per component the AI must extract: **title**, **summary**, **capabilities**, **keywords**,
**searchPhrases**, **technologies**, **dependencies**. **Capabilities** and **searchPhrases** are
the most important — they power Discover search.

---

## 5. Component model

One stored record per workflow file of a supported type.

```json
{
  "path": "skills/google-login.md",
  "githubUrl": "https://github.com/owner/repo/blob/<commit_sha>/skills/google-login.md",
  "componentType": "skill",
  "title": "",
  "summary": "",
  "capabilities": [{ "name": "", "description": "" }],
  "keywords": [],
  "searchPhrases": [],
  "technologies": [],
  "dependencies": []
}
```

### GitHub file links — backend only

The AI returns **`path` only** (repo-relative). It must **not** generate GitHub URLs.

Backend builds:

```text
repo_url + commit_sha + path
```

Benefits: no hallucinated URLs, direct file viewing from Discover, users can inspect a component
before installing.

### Search principle

Users search **capabilities and use cases**, not filenames.

| Bad | Good |
|---|---|
| `authentication.md` | `google login`, `jwt authentication`, `review pull requests` |

Every component **must** have meaningful **capabilities**, **keywords**, and **searchPhrases**.

---

## 6. Data stored after publish

### Workflow

| Field | Source |
|---|---|
| Repository URL, commit SHA | Backend (on publish) |
| Source IDE | Step 1 |
| Workflow name, description, tags | Step 3 (from AI, user-edited) |
| Dependencies (required/optional) | Step 3 (from AI, user-edited) |
| Complexity, target audience | Step 3 |
| Install count | Starts at 0; incremented on install |

### Components

One record per `rule` / `command` / `hook` / `skill` / `subagent` file, with full searchable
metadata from the AI output. Non-matching repo files are **not** indexed (see
[Discover §5.2](discover-module.md)) but are included when the whole workflow is installed.

---

## 7. Backend validation summary

| When | Checks |
|---|---|
| Step 1 (background) | Repo exists, public, accessible |
| Step 2 | Valid JSON, components present, valid types, required fields |
| Step 3 | Unique workflow name |
| Publish | Resolve commit SHA, build `githubUrl` per component, persist + index |

---

## 8. Decisions — resolved

1. ✅ **No backend repo analysis in MVP** — user runs external AI, pastes JSON.
2. ✅ **Five component types only:** `rule`, `command`, `hook`, `skill`, `subagent`.
3. ✅ **AI returns paths; backend builds GitHub URLs** from commit SHA.
4. ✅ **Name uniqueness** checked at Step 3, not Step 2.
5. ✅ **Repo validation is non-blocking** after Step 1 Next.
6. ✅ **Analysis prompt** maintained separately in `docs/prompts/repository-analysis-prompt.md`.

---

## Definition of done

- [x] Four-step wizard: repo info → AI paste → metadata review → review & publish.
- [x] Copyable analysis prompt with `{{REPOSITORY_URL}}` substitution.
- [x] JSON validation with clear errors on Step 2.
- [x] Name uniqueness with conflict UX on Step 3.
- [x] Dependency editor (add/remove, required/optional).
- [x] Complexity + target audience selectors; ownership checkbox enforced.
- [x] Publish resolves commit SHA, generates component GitHub URLs, persists workflow + components.
- [ ] Published workflows appear in Discover search with component metadata indexed.
