---
name: google-login
description: >-
  Implement or debug Google Sign-In (GIS) for Workflow Hub: Google Cloud setup,
  Angular GIS service, backend ID token verification, tokens, and common console errors.
---

# Google Login (Workflow Hub)

Use this skill when adding, fixing, or explaining Google Sign-In in this repo.

## What you need before coding

### Google Cloud Console

1. Create or use a project → **APIs & Services** → **Credentials**.
2. Create an **OAuth 2.0 Client ID** of type **Web application** (not Android/iOS).
3. **Authorized JavaScript origins** must include the **exact** origin the browser uses:
   - `http://localhost:4290` (project default dev port)
   - `http://localhost:4200` only if you intentionally run `npm start -- --port 4200`
   - Production origin when deployed (e.g. `https://your-domain.com`)
4. You do **not** need a redirect URI for GIS One Tap / button flow (ID token in browser).
5. Copy the **Client ID** (`*.apps.googleusercontent.com`).

### Frontend config

| Item | Location |
|------|----------|
| GIS script | `frontend/src/index.html` — `https://accounts.google.com/gsi/client` |
| Client ID | `frontend/src/environments/environment.development.ts` → `googleClientId` |
| Production | `frontend/src/environments/environment.ts` — set via build/replace, never commit secrets beyond public client ID |

### Backend config

| Item | Location |
|------|----------|
| Same Client ID | `appsettings.Development.json` → `Google:ClientId` (must match frontend audience) |
| Verifier | `GoogleTokenVerifier.cs` — validates JWT with `GoogleJsonWebSignature.ValidateAsync` |
| Login endpoint | `POST /api/auth/external` with Google ID token → issues access + refresh JWT |

Client ID is public; **never** put Google client **secret** in the Angular app.

## Architecture (this repo)

```
Browser (GIS)
  → ID token (JWT)
  → AuthApiService.loginWithGoogle(idToken)
  → POST /api/auth/external
  → GoogleTokenVerifier + user upsert + JWT pair
  → TokenStorage (sessionStorage)
  → interceptors attach Bearer; refresh via /api/auth/external refresh path
```

### Key frontend files

- `frontend/src/app/core/auth/google-identity.service.ts` — GIS wrapper
- `frontend/src/app/core/auth/auth.service.ts` — `loginWithGoogleIdToken`, `hardLogout` calls `googleIdentity.resetSession()`
- `frontend/src/app/features/auth/login/login.page.ts` — `promptOrRender()` → `switchMap` to API
- `frontend/src/app/core/auth/token-storage.ts` — `sessionStorage` keys `workflowhub.auth.access` / `.refresh`

### GIS service rules (important)

- Call `google.accounts.id.initialize()` **once per page load**. GIS logs a warning if called repeatedly.
- Route credentials with a **delegating callback**: set `activeHandler` per sign-in attempt, then `cancel()` + `prompt()` (or `renderButton`).
- On logout: `resetSession()` → `cancel()`, `disableAutoSelect()`, clear `activeHandler` so the next login can prompt again.
- Do **not** set a permanent `initialized` flag that blocks re-prompt without clearing GIS state.

### Login page pattern

```typescript
this.googleIdentity.promptOrRender().pipe(
  switchMap((idToken) => this.auth.loginWithGoogleIdToken(idToken)),
  finalize(() => this.loading.set(false))
).subscribe({ next: () => navigate(...), error: ... });
```

Use `switchMap` (not nested subscribe) so a second click cancels the prior attempt cleanly.

## Troubleshooting console messages

### `GET .../gsi/status?client_id=... 403 (Forbidden)`

**Cause:** Origin not allowed in Google Console, or browser URL does not match listed origins.

**Fix:**

1. Note the origin in the address bar (`http://localhost:4200` vs `:4290`).
2. Add that exact origin under **Authorized JavaScript origins** (scheme + host + port).
3. Wait a few minutes for Google to propagate; hard-refresh.

### `google.accounts.id.initialize() is called multiple times`

**Cause:** Re-initializing GIS on every login attempt.

**Fix:** Single `initialize` with per-attempt handler delegation (see `GoogleIdentityService`).

### `[GSI_LOGGER]: FedCM ... may stop functioning`

Informational migration notice from Google. Optional: follow [FedCM migration](https://developers.google.com/identity/gsi/web/guides/fedcm-migration). Not a Workflow Hub bug.

### `NG01352: ngModel within a form`

**Cause:** `[(ngModel)]` on an input inside `<form>` without `name` or `standalone: true`.

**Fix:** Prefer `[value]` + `(input)` (see `workflows-topbar.component`) or add `name="..."` / `[ngModelOptions]="{standalone: true}"`.

### Second sign-in stuck on “Signing in…”

**Cause:** GIS callback still tied to a completed Observable subscription.

**Fix:** Delegating callback + `resetSession()` on logout; fresh `activeHandler` each `promptOrRender()`.

## Dev port consistency

Workflow Hub standard frontend port is **4290** (see `frontend-dev-server` skill). If the user runs port **4200**, Google Console must list `http://localhost:4200`. Mismatch between browser port and Console origins is the most common 403 cause.

## Verification checklist

- [ ] GIS script loads (`window.google.accounts.id` defined)
- [ ] `environment.googleClientId` matches Console Web client ID
- [ ] Backend `Google:ClientId` matches frontend
- [ ] Authorized JavaScript origins include current dev URL
- [ ] Login → discover works; logout → login again works
- [ ] No NG01352 on workflows layout after login

## Commands

```bash
# Frontend (preferred port)
cd frontend && npm start -- --port 4290

# Backend
cd backend && dotnet run --project src/WorkflowHub.Api
```
