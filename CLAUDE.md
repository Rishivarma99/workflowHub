# CLAUDE.md — Workflow Hub Frontend

Guidance for Claude Code when working in this repository.

---

## Rule 0 — follow `ai-rules/` (MANDATORY)

The coding-standards library lives in a **sibling repo**:

- Relative: `../ai-rules/`
- Absolute: `C:\Users\rishi.alluri\OneDrive - McKissock LP\Documents\Projects\ai-rules`

**Every code change must follow those rules.** Do **not** load the whole tree:

1. Start at `../ai-rules/RULES-INDEX.md` (master index).
2. Open `../ai-rules/frontend/00-index/frontend-rules-index.md` for frontend tasks.
3. Load **only** the 2–5 rule files that match the task, plus these always-on frontend rules:
   - `frontend/01-architecture/folder-structure-and-boundary-rules.md`
   - `frontend/05-quality/naming-and-file-rules.md`
   - `frontend/05-quality/forbidden-patterns-rules.md`

> If `../ai-rules/` is missing: `git clone https://gitlab.pal.tech/rishi.alluri/ai-rules.git ../ai-rules`

---

## What this project is

**Workflow Hub UI** — Angular app to publish and discover IDE workflows pinned to public GitHub
repos. Talks to the Workflow Hub API in the sibling repo `../workflowHub-backend/`.

Full product spec: [`workflowHub-prd-v2.md`](workflowHub-prd-v2.md).

---

## Tech stack

| Layer | Stack |
|---|---|
| **Framework** | Angular 20 (standalone, application builder) |
| **Styling** | Tailwind CSS, PrimeNG, SCSS |
| **API** | REST via `environment.apiBaseUrl` (proxy in local dev) |

---

## Design reference (MANDATORY for all UI/design work)

For **every** UI or design decision — layout, spacing, typography, colors, components, screen
structure, states, and responsive behavior — consult the sibling design-reference repo first:

- Relative: `../workflowHub-design-reference/`
- Absolute: `C:\Users\rishi.alluri\OneDrive - McKissock LP\Documents\Projects\workflowHub-design-reference`
- Not a git repo — local sibling folder only

Key files (read the relevant ones before implementing a screen):

| File | Purpose |
|---|---|
| `colors_and_type.css` | Design tokens — fonts, type scale, color palette |
| `app.css` | Layout, components, shell styling |
| `app.jsx` | App shell, routing, sidebar/nav |
| `ui.jsx` | Shared UI primitives |
| `screens1.jsx` … `screens4.jsx` | Screen implementations |
| `data.jsx` | Mock data shapes |
| `Workflow Hub.html` | Entry point to preview all screens |

Re-express React/JSX in Angular + Tailwind + PrimeNG idioms. **Never** ship or import reference code.
See [`design-reference/README.md`](design-reference/README.md) and `.cursor/rules/design-reference.mdc`.

---

## Repository layout

```
workflowHub-frontend/         <- THIS repo (rename from workflowHub when the folder is not in use)
├── frontend/                 <- Angular app
│   └── src/app/
│       ├── core/             <- singletons: services, guards, interceptors
│       ├── shared/           <- reusable UI, pipes, models
│       └── features/         <- auth, workflows, home
└── design-reference/         <- pointer doc to sibling design-reference repo
```

**Component prefix:** `wh` (e.g. `wh-root`).

---

## Environments

| Context | Angular config | API |
|---|---|---|
| Local dev | `environment.development.ts` | `/api` via `proxy.conf.json` → Render dev or local API |
| Netlify dev | `environment.dev.ts` (when added) | `/api` via Netlify proxy |
| Netlify staging | `environment.staging.ts` (when added) | `/api` via Netlify proxy |
| Netlify prod | `environment.ts` | `/api` via Netlify proxy |

---

## Commands

```bash
cd frontend
npm install
npm start -- --port 4290    # http://localhost:4290/
npm run build
npm test
```

---

## Working agreements

- **Rules first** — load frontend rules from `../ai-rules/` via `RULES-INDEX.md` (see `.cursor/rules/ai-rules.mdc`).
- **Design first** — consult `../workflowHub-design-reference/` for every UI/design decision.
- **Match the PRD** for feature behavior; coordinate API contract changes with `workflowHub-backend`.
- **No secrets in source control** — Google client ID is public; never commit API keys or JWT secrets.
- Profile pictures use Google `avatar_url` from login — no uploads.
