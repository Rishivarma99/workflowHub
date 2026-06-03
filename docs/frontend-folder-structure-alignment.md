# Frontend folder structure — alignment backlog

> **Status:** Settings refactor done (June 2026). Auth + discover placeholders still open.  
> **Rules:** [`../../ai-rules/frontend/01-architecture/folder-structure-and-boundary-rules.md`](../../ai-rules/frontend/01-architecture/folder-structure-and-boundary-rules.md)

---

## Product naming (Settings vs “user management”)

| Term | Meaning in Workflow Hub |
|------|-------------------------|
| **Settings** | One screen: **your** profile & account (design reference `screens4.jsx`). Route: `/workflows/settings`. |
| **User management** | **Do not use** for this screen — implies admin listing/editing all users. Not in MVP PRD. |

Implemented tree:

```text
features/workflows/settings/
  settings.routes.ts
  pages/
    settings-page.component.ts   # SettingsPageComponent, selector wh-settings-page
  components/
    settings-toggle.component.ts
  state/
    settings-notification-prefs.service.ts
```

---

## Still to do (other areas)

### Auth

```text
# Current
features/auth/login/login.page.ts

# Target
features/auth/pages/login-page.component.ts
```

### Discover / search / mine / create

Prefer one folder per route with `pages/` when implementing; interim placeholder under `workflows/shared/pages/` per Rule 8.7.

---

## Confusion index (see ai-rules §13)

| # | Topic | Status |
|---|--------|--------|
| 1 | `{feature}.routes.ts` | OK |
| 2 | `*-page.component.ts` | Settings OK; auth login still `.page.ts` |
| 3 | `core/auth` vs `features/auth` | OK by design |
| 4 | Shared placeholder routes | Open |
| 5 | `settings/user-management/` nesting | **Fixed** → `settings/pages/settings-page` |
| 6–9 | forms/, paths, API | See ai-rules 8.6–8.9 |
