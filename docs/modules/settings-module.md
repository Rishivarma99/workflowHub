# Settings Module

**Status:** Spec drafted · **Last updated:** 30 May 2026
**Depends on:** Auth module (a logged-in user + our JWT).

---

## 1. Purpose & placement

The **Settings** module is the area where a logged-in user manages their account. It is reached
from the **app sidebar → Settings**. Settings has its own inner navigation (a sub-sidebar or tabs);
its first and only MVP sub-section is **User Management**.

```
App sidebar
  ├── Workflows
  ├── Search
  └── Settings            <- opens the Settings module
        └── User Management   <- MVP (this doc)
        └── (future: Notifications, Connected accounts, Danger zone…)
```

## 2. Sub-modules

| Sub-module | Scope | Status |
|---|---|---|
| **User Management** | The logged-in user's **own** profile/account. View details, edit display name + bio, upload a profile picture. | 🟡 this doc |
| Notifications | Email/in-app notification prefs | ⚪ future |
| Connected accounts | Google connection info, disconnect | ⚪ future |
| Danger zone | Delete account | ⚪ future |

> **Scope note:** User Management is **self-service for the current user only** — *not* an admin
> screen that lists/manages all users. MVP has no roles/teams (per PRD non-goals), so there is no
> "manage other users" view. If that's ever needed, it becomes its own module with a role model.

---

## 3. User Management (My Profile)

### 3.1 Purpose
Let the signed-in user see their account details and personalize their public identity (display
name, short bio, profile picture). Email comes from Google and is **read-only**.

### 3.2 Screens

| Screen | Route | What it shows |
|---|---|---|
| **Profile** | `/settings/user-management` | Profile header (avatar, display name, email, joined date) + an editable profile form + avatar upload control. |

UI states to handle on this screen (per error-handling rules): **loading** (fetching `/me`),
**ready**, **saving** (form submit / upload in progress), **success** (toast), **error** (inline +
toast), **empty** N/A (always a user).

### 3.3 Data shown

| Field | Source | Editable? | Notes |
|---|---|---|---|
| Avatar | `users.avatar_url` | ✅ upload/replace/remove | Google avatar by default; custom upload overrides it. |
| Display name | `users.display_name` ?? `users.name` | ✅ | Falls back to the Google name if not set. |
| Email | `users.email` | ❌ read-only | From Google; shown disabled. |
| Short bio | `users.bio` | ✅ | Optional, ≤ 160 chars. |
| Joined | `users.created_at` | ❌ | Display only (e.g. "Joined May 2026"). |

### 3.4 Forms & validation

**Edit Profile form** (reactive form):

| Control | Type | Rules |
|---|---|---|
| `displayName` | text input | optional; trimmed; **2–50** chars when present; defaults to Google name. |
| `bio` | textarea | optional; **≤ 160** chars; trimmed; plain text (no HTML). |
| `email` | text input | **disabled** (display only, never submitted). |

- Submit is enabled only when the form is **dirty + valid**.
- On submit → `PATCH /me` with `{ displayName, bio }` → update local user state → success toast.
- Validation messages shown inline under each control (forms + error-handling rules).

**PrimeNG components:** `InputText` (displayName), `Textarea` (bio) with a live char counter,
`Button` (Save), `Avatar` (display), `FileUpload` (avatar), `Toast` (feedback).

### 3.5 Profile picture — upload & storage

**Storage decision:** images live in **Supabase Storage** (free tier, S3-compatible object
storage). We use a public bucket named **`avatars`** and store only the resulting **public URL**
in `users.avatar_url`. No image bytes in Postgres.

**Client-side rules (before upload):**
- Accepted types: `image/jpeg`, `image/png`, `image/webp`.
- Max size: **2 MB**.
- Show a live preview before confirming.

**Server-side flow (`POST /me/avatar`, `multipart/form-data`, field `file`):**
```
1. AuthN/Z   require valid JWT; target = current user only.
2. VALIDATE  content-type ∈ {jpeg,png,webp}; size ≤ 2 MB; reject otherwise (400).
3. PROCESS   (recommended) decode → strip EXIF → resize/crop to a square ≤ 512×512 → re-encode webp.
4. PATH      bucket `avatars`, object path  <userId>/<newUuid>.webp   # new uuid = auto cache-bust
5. UPLOAD    backend uploads to Supabase Storage with the SERVICE-ROLE key:
                POST {SUPABASE_URL}/storage/v1/object/avatars/<userId>/<newUuid>.webp
             public URL =
                {SUPABASE_URL}/storage/v1/object/public/avatars/<userId>/<newUuid>.webp
6. PERSIST   old = users.avatar_url (if it was one of our Supabase objects)
             users.avatar_url = <new public URL>; users.updated_at = now
7. CLEANUP   best-effort delete the previous object (Supabase: DELETE /storage/v1/object/...)
             only if it was ours (not the Google URL).
8. RETURN    updated user (200).
```

**Remove picture (`DELETE /me/avatar`):** delete the custom object from the `avatars` bucket
(best-effort) and reset `avatar_url` back to the Google avatar URL (kept on the user as
`google_avatar_url`, see §3.6) or to `null` so the UI shows initials.

**Supabase config / secrets (out of source control):**
- `SUPABASE_URL` — project URL (e.g. `https://<ref>.supabase.co`).
- `SUPABASE_SERVICE_ROLE_KEY` — **server-side only**, used by the .NET backend to upload/delete
  (bypasses RLS). Never expose to the frontend.
- Bucket: `avatars` (public). Created once via the Supabase dashboard or a setup script.
- .NET integration: call the Supabase **Storage REST API** over HTTP (simple, no extra SDK), or use
  the `Supabase.Storage`/`supabase-csharp` client, or the S3-compatible endpoint with the AWS SDK —
  any works since Supabase Storage is S3-compatible. REST is the lightest for MVP.

**Notes & guards:**
- The `avatars` bucket is **public-read** (profile images are public). Reads need no auth — the
  stored public URL is served directly by Supabase's CDN.
- Uploads/deletes happen **only through our backend** with the service-role key, so the frontend
  never holds Supabase credentials. (Alternative — signed upload URLs / direct-from-browser — noted
  in §3.10.)
- Enforce the size limit at the web-server/body level too, not just app code. (Supabase also has a
  per-bucket file-size limit that can be set as a backstop.)

### 3.6 Data-model changes

Extends the PRD `User` entity. **New/changed columns:**

```
User
  id            (uuid, pk)             # existing
  google_sub    (string, unique)       # existing
  email         (string, unique)       # existing — read-only in UI
  name          (string)               # existing — original Google name (kept as source)
  display_name  (string, null)         # NEW — editable; UI shows display_name ?? name
  bio           (string(160), null)    # NEW — editable short bio
  avatar_url    (string, null)         # existing — now Google URL or our uploaded URL
  google_avatar_url (string, null)     # NEW — preserves Google's avatar so "remove" can revert
  created_at    (timestamptz)          # existing
  updated_at    (timestamptz)          # NEW — bumped on profile edits/avatar changes
```

→ Needs an EF Core migration adding `display_name`, `bio`, `google_avatar_url`, `updated_at`.
At Google login, set `google_avatar_url` (and `avatar_url` if the user has no custom one yet).

### 3.7 API surface

| Method | Route | Auth | Body | Returns |
|---|---|---|---|---|
| `GET` | `/me` | JWT | — | current user (now incl. `displayName`, `bio`, `avatarUrl`, `email`, `createdAt`) |
| `PATCH` | `/me` | JWT | `{ displayName?, bio? }` | updated user |
| `POST` | `/me/avatar` | JWT | multipart `file` | updated user (new `avatarUrl`) |
| `DELETE` | `/me/avatar` | JWT | — | updated user (reverted `avatarUrl`) |

All wrapped in the standard `ApiResponse<T>` envelope. `GET /me` already exists in the PRD (§6) —
this extends its payload and adds the three write endpoints.

### 3.8 Frontend structure (Angular)

```
frontend/src/app/
  features/settings/
    settings.routes.ts                 # lazy SETTINGS_ROUTES
    settings-layout.component.ts        # inner nav (User Management + future) + router-outlet
    user-management/
      profile.page.ts                  # the screen (page component)
      components/
        profile-form.component.ts       # display name + bio reactive form
        avatar-uploader.component.ts     # PrimeNG FileUpload + preview + validation
      user-profile.service.ts           # GET/PATCH /me, POST/DELETE /me/avatar
  core/
    services/current-user.store.ts      # signal-based current-user state (shared app-wide)
```

- Route: `/settings/user-management`, lazy-loaded; guarded by the auth guard (must be logged in).
- Current-user state in a **signal store** so the avatar/name update reflects in the sidebar/header
  immediately after a save (signals-and-rxjs rules).
- Avatar uploader does client-side validation, shows preview, posts multipart, updates the store.

### 3.9 Rules to follow (load these from `../../../ai-rules/`)

Per `RULES-INDEX.md` task rows — load only these for this module:
- **Frontend:** `02-angular/forms-rules.md`, `02-angular/component-and-template-rules.md`,
  `02-angular/signals-and-rxjs-rules.md`, `03-data/api-and-http-rules.md`,
  `03-data/dto-mapper-model-rules.md`, `03-data/error-handling-rules.md`,
  `04-ui-styling/primeng-styling-rules.md`, `04-ui-styling/scss-and-design-token-rules.md`
  (+ always-on: folder-structure, naming, forbidden-patterns).
- **Backend:** `02-api/api-layer-rules.md`, `02-api/api-dto-mapping-rules.md`,
  `03-application/command-rules.md` + `handler-rules.md` (for PATCH/avatar),
  `03-application/query-rules.md` (for GET /me), `03-application/validation-rules.md`,
  `04-infrastructure/entity-design-and-navigation-rules.md` + `validation-and-ef-configuration-rules.md`
  (+ always-on: architecture-overview, core-type, cqrs-overview).

### 3.10 Open questions

1. Supabase `avatars` bucket **public** (store public URL — chosen) vs **private** (signed URLs)? Default: public.
   Also: upload **through our backend** (chosen) vs **direct-from-browser via signed upload URL**? Default: backend.
2. Server-side image **resize/crop** in v1, or accept-as-is (with size cap) and defer processing?
3. On "remove picture": revert to **Google avatar** or to **initials placeholder**? Default: Google, else initials.
4. Is `display_name` a separate column (proposed) or should we just make `name` editable? Default: separate.

---

## Definition of done (User Management)

- [ ] `GET /me` returns the extended profile payload.
- [ ] User can edit display name + bio (`PATCH /me`) with validation; changes persist and reflect app-wide.
- [ ] User can upload a profile picture (validated, stored in object storage, `avatar_url` updated).
- [ ] User can remove the custom picture and revert.
- [ ] Email is shown read-only; joined date displayed.
- [ ] Migration applied for the new columns.
- [ ] Screen handles loading/saving/error states with clear feedback.
