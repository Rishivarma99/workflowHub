# Auth And User Management Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build Google-only authentication with backend-minted JWTs, protected Workflow Hub routes, and the Option B self-service user-management profile slice without custom avatar storage.

**Architecture:** The backend verifies Google ID tokens, stores users and refresh tokens in Postgres through EF Core, issues short-lived access tokens, and exposes `/auth/*` plus `/me` endpoints. The Angular app keeps tokens in `sessionStorage` via `TokenStorage`, centralizes auth state in `AuthService`, attaches tokens through interceptors, and gates `/workflows/*` with an auth guard. User-management profile edits are scoped to the current user only; avatars come from the Google profile picture URL captured during login.

**Tech Stack:** .NET 8, EF Core + Npgsql, ASP.NET Core JWT bearer auth, Google ID token verification, Angular 20 standalone components, signals, RxJS, PrimeNG/Tailwind-compatible styling.

---

## Scope

Included:
- Google-only sign-in through `POST /auth/google`.
- Backend access token + refresh token issuance.
- Refresh token rotation and logout revocation.
- Protected `/me` and `PATCH /me`.
- Frontend `AuthService`, token storage, auth/refresh interceptors, auth guard.
- Login page wired to Google Identity Services.
- User Management profile screen at `/workflows/settings/user-management` or equivalent child route under the existing shell.
- Google avatar URL display only; no custom avatar upload in this slice.

Excluded:
- Email/password, GitHub OAuth, teams, roles, admin user listing.
- Custom avatar upload/storage.
- Real notification preferences.

## File Structure

Backend files:
- Create `backend/src/WorkflowHub.Data/Entities/User.cs` — persisted Google-backed user profile.
- Create `backend/src/WorkflowHub.Data/Entities/RefreshToken.cs` — hashed refresh token rows.
- Create `backend/src/WorkflowHub.Data/Persistence/Configurations/UserConfiguration.cs` — EF constraints and column mapping.
- Create `backend/src/WorkflowHub.Data/Persistence/Configurations/RefreshTokenConfiguration.cs` — EF constraints and indexes.
- Modify `backend/src/WorkflowHub.Data/Persistence/AppDbContext.cs` — expose `Users` and `RefreshTokens`, apply configurations.
- Create `backend/src/WorkflowHub.Application/Auth/Models/*.cs` — auth DTOs and current-user model.
- Create `backend/src/WorkflowHub.Application/Auth/Services/*.cs` — token, Google verifier, refresh token, current-user services.
- Create `backend/src/WorkflowHub.Api/Controllers/AuthController.cs` — `/auth/google`, `/auth/refresh`, `/auth/logout`.
- Create `backend/src/WorkflowHub.Api/Controllers/MeController.cs` — `/me` profile APIs.
- Modify `backend/src/WorkflowHub.Api/Extensions/ApiServicesExtensions.cs` — register JWT auth, Google options, auth services.
- Modify `backend/src/WorkflowHub.Api/Extensions/PresentationExtensions.cs` — add `UseAuthentication()` before `UseAuthorization()`.
- Modify `backend/src/WorkflowHub.Api/appsettings.json` and `appsettings.Development.json` — non-secret option shape only.

Frontend files:
- Create `frontend/src/app/core/auth/token-storage.ts` — sessionStorage ownership.
- Create `frontend/src/app/core/auth/auth-context.ts` — `SKIP_AUTH` and `skipAuth()`.
- Create `frontend/src/app/core/auth/auth.models.ts` — `User`, `TokenPair`, request/response types.
- Create `frontend/src/app/core/auth/auth.service.ts` — login, refresh, logout, current user signal.
- Create `frontend/src/app/core/auth/auth.interceptor.ts` — attach access token to API requests.
- Create `frontend/src/app/core/auth/refresh.interceptor.ts` — single-flight refresh on 401.
- Create `frontend/src/app/core/guards/auth.guard.ts` — `canMatch` for workflow routes.
- Modify `frontend/src/app/app.config.ts` — provide `HttpClient` and interceptors.
- Modify `frontend/src/app/app.routes.ts` — apply auth guard to `/workflows`.
- Modify `frontend/src/app/features/auth/login/login.page.ts` — load GIS and call `AuthService.loginWithGoogleIdToken`.
- Create `frontend/src/app/features/workflows/settings/user-management/*` — profile page and focused child components.
- Modify `frontend/src/app/features/workflows/workflows.routes.ts` — add settings child route.
- Modify `frontend/src/app/features/workflows/layout/workflows-layout.component.ts` — bind shell user/sign-out to `AuthService`.

Test files:
- Create `backend/tests/WorkflowHub.Application.Tests/Auth/*.cs`.
- Create `backend/tests/WorkflowHub.Api.Tests/Auth/*.cs`.
- Create frontend specs beside new auth/profile files where behavior is non-trivial.

---

## Task 1: Backend Persistence Model

**Files:**
- Create: `backend/src/WorkflowHub.Data/Entities/User.cs`
- Create: `backend/src/WorkflowHub.Data/Entities/RefreshToken.cs`
- Create: `backend/src/WorkflowHub.Data/Persistence/Configurations/UserConfiguration.cs`
- Create: `backend/src/WorkflowHub.Data/Persistence/Configurations/RefreshTokenConfiguration.cs`
- Modify: `backend/src/WorkflowHub.Data/Persistence/AppDbContext.cs`

- [ ] **Step 1: Add `User` entity**

```csharp
namespace WorkflowHub.Data.Entities;

public sealed class User
{
    public Guid Id { get; set; }
    public string GoogleSub { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? GoogleAvatarUrl { get; set; }
    public string SecurityStamp { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
```

- [ ] **Step 2: Add `RefreshToken` entity**

```csharp
namespace WorkflowHub.Data.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public bool IsUsed { get; set; }
    public string? DeviceInfo { get; set; }
    public User User { get; set; } = default!;
}
```

- [ ] **Step 3: Configure EF mappings**

`UserConfiguration` must enforce unique `GoogleSub` and `Email`, max lengths for profile fields, and UTC timestamp columns. `RefreshTokenConfiguration` must index `UserId`, `TokenHash`, and active token lookup fields.

- [ ] **Step 4: Update `AppDbContext`**

```csharp
public DbSet<User> Users => Set<User>();
public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

- [ ] **Step 5: Create EF migration**

Run:

```bash
dotnet ef migrations add AddAuthUsers --project backend/src/WorkflowHub.Data --startup-project backend/src/WorkflowHub.Api
```

Expected: migration creates `Users` and `RefreshTokens` tables.

- [ ] **Step 6: Commit**

```bash
git add backend/src/WorkflowHub.Data backend/src/WorkflowHub.Api backend/WorkflowHub.sln
git commit -m "feat: add auth persistence model"
```

---

## Task 2: Backend Token And Google Verification Services

**Files:**
- Modify: `backend/src/WorkflowHub.Api/WorkflowHub.Api.csproj`
- Create: `backend/src/WorkflowHub.Application/Auth/Models/AuthModels.cs`
- Create: `backend/src/WorkflowHub.Application/Auth/Services/GoogleTokenVerifier.cs`
- Create: `backend/src/WorkflowHub.Application/Auth/Services/JwtTokenService.cs`
- Create: `backend/src/WorkflowHub.Application/Auth/Services/RefreshTokenService.cs`
- Modify: `backend/src/WorkflowHub.Application/Bootstrap/BusinessBootstrapper.cs`
- Modify: `backend/src/WorkflowHub.Api/Extensions/ApiServicesExtensions.cs`

- [ ] **Step 1: Add backend packages**

Run:

```bash
dotnet add backend/src/WorkflowHub.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add backend/src/WorkflowHub.Application package Google.Apis.Auth
```

Expected: project files include the packages.

- [ ] **Step 2: Define auth models**

Create records:

```csharp
public sealed record GoogleLoginRequest(string IdToken);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record TokenPair(string AccessToken, string RefreshToken);
public sealed record AuthUserDto(Guid Id, string Email, string Name, string? DisplayName, string? Bio, string? AvatarUrl, DateTime CreatedAtUtc);
public sealed record AuthResult(TokenPair Tokens, AuthUserDto User);
```

- [ ] **Step 3: Implement `GoogleTokenVerifier`**

Use `GoogleJsonWebSignature.ValidateAsync(idToken, new ValidationSettings { Audience = [clientId] })`. Return `sub`, `email`, `name`, `picture`. Reject missing email or subject with a validation exception mapped to `400`.

- [ ] **Step 4: Implement `JwtTokenService`**

Issue access tokens with claims:

```csharp
new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
new Claim(JwtRegisteredClaimNames.Email, user.Email),
new Claim("security_stamp", user.SecurityStamp)
```

Use configured issuer, audience, signing key, and 15-minute expiry.

- [ ] **Step 5: Implement `RefreshTokenService`**

Generate 32 random bytes, base64url encode them, hash with SHA-256 before storing, set expiry to configured days, mark `IsUsed = true` on refresh rotation, and set `RevokedAtUtc` on logout.

- [ ] **Step 6: Register services**

`BusinessBootstrapper.Register` registers Google verifier, JWT token service, and refresh token service. `ApiServicesExtensions.AddApiServices` binds JWT/Google options and registers ASP.NET authentication.

- [ ] **Step 7: Commit**

```bash
git add backend/src/WorkflowHub.Api backend/src/WorkflowHub.Application
git commit -m "feat: add auth token services"
```

---

## Task 3: Backend Auth And Profile APIs

**Files:**
- Create: `backend/src/WorkflowHub.Api/Controllers/AuthController.cs`
- Create: `backend/src/WorkflowHub.Api/Controllers/MeController.cs`
- Modify: `backend/src/WorkflowHub.Api/Extensions/PresentationExtensions.cs`
- Create: `backend/src/WorkflowHub.Application/Auth/Services/CurrentUserService.cs`

- [ ] **Step 1: Add auth pipeline**

In `UsePresentationPipeline`, the order must be:

```csharp
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
```

- [ ] **Step 2: Implement `POST /auth/google`**

Flow:
1. Verify Google ID token.
2. Find user by `GoogleSub`.
3. If missing, create user with `Email`, `Name`, `GoogleAvatarUrl`, `AvatarUrl`, timestamps, security stamp.
4. If present, update Google name/email/avatar source fields without overwriting custom `DisplayName`, `Bio`, or custom avatar.
5. Issue access token and refresh token.
6. Return `ApiResponse<AuthResult>`.

- [ ] **Step 3: Implement `POST /auth/refresh`**

Validate refresh token hash, ensure not expired/revoked/used, mark old token used, issue a new token pair, return `ApiResponse<TokenPair>`.

- [ ] **Step 4: Implement `POST /auth/logout`**

Validate refresh token hash if present, revoke it if found, return `ApiResponse<object>` with `Data = null`.

- [ ] **Step 5: Implement `/me` endpoints**

`GET /me` reads user id from `sub`, returns profile DTO. `PATCH /me` accepts `{ displayName, bio }`, trims values, validates display name length 2-50 when present, validates bio length <= 160, updates `UpdatedAtUtc`, returns updated user.

- [ ] **Step 6: Keep avatar read-only from Google**

Do not add `POST /me/avatar` or `DELETE /me/avatar` in this slice. During `POST /auth/google`, store the Google `picture` claim in `GoogleAvatarUrl` and set `AvatarUrl` to the same value. `GET /me` returns `avatarUrl` so the frontend can render the Google avatar. If Google later sends a changed picture URL, update both `GoogleAvatarUrl` and `AvatarUrl` because there is no custom avatar override yet.

- [ ] **Step 7: Verify backend build**

Run:

```bash
dotnet build backend/WorkflowHub.sln
```

Expected: build succeeds.

- [ ] **Step 8: Commit**

```bash
git add backend/src
git commit -m "feat: add auth and profile APIs"
```

---

## Task 4: Frontend Auth Core

**Files:**
- Modify: `frontend/src/app/app.config.ts`
- Modify: `frontend/src/app/app.routes.ts`
- Create: `frontend/src/app/core/auth/token-storage.ts`
- Create: `frontend/src/app/core/auth/auth-context.ts`
- Create: `frontend/src/app/core/auth/auth.models.ts`
- Create: `frontend/src/app/core/auth/auth.service.ts`
- Create: `frontend/src/app/core/auth/auth.interceptor.ts`
- Create: `frontend/src/app/core/auth/refresh.interceptor.ts`
- Create: `frontend/src/app/core/guards/auth.guard.ts`

- [ ] **Step 1: Add token storage**

```ts
@Injectable({ providedIn: 'root' })
export class TokenStorage {
  private readonly accessKey = 'workflowhub.auth.access';
  private readonly refreshKey = 'workflowhub.auth.refresh';

  getAccess(): string | null {
    return sessionStorage.getItem(this.accessKey);
  }

  getRefresh(): string | null {
    return sessionStorage.getItem(this.refreshKey);
  }

  setTokens(accessToken: string, refreshToken: string): void {
    sessionStorage.setItem(this.accessKey, accessToken);
    sessionStorage.setItem(this.refreshKey, refreshToken);
  }

  clear(): void {
    sessionStorage.removeItem(this.accessKey);
    sessionStorage.removeItem(this.refreshKey);
  }
}
```

- [ ] **Step 2: Add `SKIP_AUTH` context**

```ts
export const SKIP_AUTH = new HttpContextToken<boolean>(() => false);
export const skipAuth = () => ({ context: new HttpContext().set(SKIP_AUTH, true) });
```

- [ ] **Step 3: Implement `AuthService`**

Methods:
- `loginWithGoogleIdToken(idToken: string): Observable<User>`
- `loadCurrentUser(): Observable<User>`
- `refresh(): Observable<TokenPair>`
- `logout(): Observable<void>`
- `hardLogout(): void`

Signals:
- `currentUser = signal<User | null>(null)`
- `isAuthenticated = computed(() => this.currentUser() !== null)`

- [ ] **Step 4: Implement interceptors**

`authInterceptor` attaches `Authorization: Bearer <access>` only for `/api/` or configured API URL and only when `SKIP_AUTH` is false. `refreshInterceptor` handles `401`, runs one refresh request at a time, retries queued requests after refresh succeeds, and calls `hardLogout()` if refresh fails.

- [ ] **Step 5: Add guard**

`authGuard` returns true when an access token exists and `loadCurrentUser()` succeeds; otherwise redirects to `/auth/login`.

- [ ] **Step 6: Wire providers and route guard**

Use `provideHttpClient(withInterceptors([authInterceptor, refreshInterceptor]))` in `app.config.ts`. Add `canMatch: [authGuard]` to the `/workflows` top-level route.

- [ ] **Step 7: Commit**

```bash
git add frontend/src/app/core frontend/src/app/app.config.ts frontend/src/app/app.routes.ts
git commit -m "feat: add frontend auth core"
```

---

## Task 5: Frontend Login Wiring

**Files:**
- Modify: `frontend/src/index.html`
- Modify: `frontend/src/app/features/auth/login/login.page.ts`
- Modify: `frontend/src/app/features/auth/login/login.page.html`

- [ ] **Step 1: Load Google Identity Services**

Add this script to `index.html`:

```html
<script src="https://accounts.google.com/gsi/client" async defer></script>
```

- [ ] **Step 2: Add `GoogleIdentityService` wrapper**

Create a small service under `core/auth/google-identity.service.ts` that initializes GIS with the configured client id and returns the ID token from the credential callback as an observable.

- [ ] **Step 3: Update login page**

The button click calls `googleIdentity.promptOrRender()`, then passes the returned ID token into `AuthService.loginWithGoogleIdToken(idToken)`, then navigates to `/workflows/discover`.

- [ ] **Step 4: Add user-facing states**

The login page must show:
- idle: enabled Google button
- loading: disabled button text `Signing in...`
- error: inline message `Google sign-in failed. Try again.`

- [ ] **Step 5: Commit**

```bash
git add frontend/src/index.html frontend/src/app/features/auth frontend/src/app/core/auth/google-identity.service.ts
git commit -m "feat: wire Google sign in"
```

---

## Task 6: Shell Auth Integration

**Files:**
- Modify: `frontend/src/app/features/workflows/layout/workflows-layout.component.ts`
- Modify: `frontend/src/app/features/workflows/layout/sidebar/workflows-sidebar.component.ts`
- Modify: `frontend/src/app/features/workflows/layout/topbar/workflows-topbar.component.ts`

- [ ] **Step 1: Replace placeholder shell user**

Use `AuthService.currentUser` to derive `ShellUser`:

```ts
readonly user = computed<ShellUser>(() => {
  const user = this.auth.currentUser();
  return {
    name: user?.displayName || user?.name || 'Workflow Hub User',
    email: user?.email || '',
    avatarColor: '#5353ef'
  };
});
```

- [ ] **Step 2: Wire sign out**

`onSignOut()` calls `this.auth.logout().subscribe()` and `hardLogout()` handles navigation on completion or error.

- [ ] **Step 3: Keep sidebar/topbar presentational**

Do not inject `AuthService` into sidebar/topbar. They receive `ShellUser` as input and emit events.

- [ ] **Step 4: Commit**

```bash
git add frontend/src/app/features/workflows/layout
git commit -m "feat: connect shell to auth state"
```

---

## Task 7: User Management Profile Screen

**Files:**
- Create: `frontend/src/app/features/workflows/settings/settings.routes.ts`
- Create: `frontend/src/app/features/workflows/settings/user-management/user-management.page.ts`
- Create: `frontend/src/app/features/workflows/settings/user-management/user-management.page.html`
- Create: `frontend/src/app/features/workflows/settings/user-management/user-management.page.scss`
- Create: `frontend/src/app/features/workflows/settings/user-management/user-profile.service.ts`
- Create: `frontend/src/app/features/workflows/settings/user-management/components/profile-form.component.ts`
- Modify: `frontend/src/app/features/workflows/workflows.routes.ts`

- [ ] **Step 1: Add route**

`/workflows/settings` redirects to `/workflows/settings/user-management`. The user-management page is lazy-loaded.

- [ ] **Step 2: Add profile API service**

Methods:
- `getMe(): Observable<User>`
- `updateProfile(body: { displayName: string | null; bio: string | null }): Observable<User>`

- [ ] **Step 3: Add profile form**

Reactive form controls:
- `displayName`: optional, trimmed, valid when empty or 2-50 chars
- `email`: disabled
- `bio`: optional, max 160 chars

Submit emits `{ displayName, bio }`.

- [ ] **Step 4: Add Google avatar display**

Render `user.avatarUrl` from `/me` in the profile header and shell. If `avatarUrl` is empty, render initials with `WhAvatarComponent`. Do not show upload, replace, or remove controls in this slice.

- [ ] **Step 5: Page orchestration**

The page loads `/me`, populates form, shows loading/saving/error states, calls profile service methods, updates `AuthService.currentUser` after successful saves, and displays success feedback.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/app/features/workflows/settings frontend/src/app/features/workflows/workflows.routes.ts
git commit -m "feat: add user management profile screen"
```

---

## Task 8: Verification Pass

**Files:**
- Modify tests created during prior tasks if needed.

- [ ] **Step 1: Backend build**

Run:

```bash
dotnet build backend/WorkflowHub.sln
```

Expected: succeeds.

- [ ] **Step 2: Backend tests**

Run:

```bash
dotnet test backend/WorkflowHub.sln
```

Expected: auth service tests and controller tests pass.

- [ ] **Step 3: Frontend checks**

Only run these after dependencies are already installed by the developer:

```bash
npx ng test --watch=false
npx ng build
```

Expected: tests and build pass.

- [ ] **Step 4: Manual auth flow**

Use configured `Google:ClientId`, `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:SigningKey`. Confirm:
- logged-out `/workflows/discover` redirects to `/auth/login`
- Google sign-in reaches `/workflows/discover`
- protected API requests include `Authorization`
- expired access token triggers refresh once
- logout clears tokens and returns to `/auth/login`
- profile save updates sidebar/topbar user name
- Google avatar URL displays in the shell/profile; users with no avatar show initials

- [ ] **Step 5: Final commit**

```bash
git add .
git commit -m "feat: complete auth and user management"
```

---

## Self-Review

- Spec coverage: PRD auth requirements map to Tasks 1-6. Settings module Option B maps to Tasks 3 and 7. Verification maps to Task 8.
- Placeholder scan: No placeholder sections; every task names files, behavior, commands, and expected outcomes.
- Type consistency: `User`, `TokenPair`, `AuthResult`, `ShellUser`, and token method names are consistent across backend/frontend tasks.
- Scope check: Authentication and current-user profile are tightly coupled for this slice. Custom avatar upload/storage, admin user management, roles, teams, notification prefs, and extra design-reference profile fields remain outside this plan.
