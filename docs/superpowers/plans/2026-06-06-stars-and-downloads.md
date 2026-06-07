# Stars & Downloads Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add binary stars (one per user, not ratings) and copy-triggered download counts for workflows, plus stars-only for agent assets (`WorkflowComponent` rows), with denormalized counters and `isStarred` on all card/detail DTOs.

**Architecture:** Two relationship tables (`workflow_stars`, `workflow_component_stars`) enforce `UNIQUE(UserId, WorkflowId|ComponentId)`. Denormalized `StarCount` (and `DownloadCount` on workflows only) are updated inside transactional commands — never computed with `COUNT(*)` on read paths. **DownloadCount increments only when the user clicks Copy clone command** on the workflow detail page — not on page load, not on a separate install/download button. Agent assets have **no** download counter or download endpoint in MVP.

**Tech Stack:** .NET 8 + EF Core + CQRS, PostgreSQL, Angular 20 standalone + signals, existing `ApiResponse<T>` envelope.

**Rules to load before coding:**
- Backend always-on: `backend/01-architecture/backend-architecture-overview.md`, `core-type-rules.md`, `cqrs-overview.md`
- Backend tasks: `command-rules.md`, `handler-rules.md`, `transaction-rules.md`, `query-rules.md`, `entity-design-and-navigation-rules.md`, `validation-and-ef-configuration-rules.md`, `api-layer-rules.md`, `api-response-contract-rules.md`, `repository-rules.md`
- Frontend always-on: `frontend/01-architecture/folder-structure-and-boundary-rules.md`, `naming-and-file-rules.md`, `forbidden-patterns-rules.md`
- Frontend tasks: `03-data/api-and-http-rules.md`, `02-angular/component-and-template-rules.md`, `02-angular/signals-and-rxjs-rules.md`, `03-data/error-handling-rules.md`

---

## Product Rules (locked for MVP)

| Entity | Stars | Downloads | Star semantics |
|---|---|---|---|
| **Workflow** | Yes | Yes | Binary toggle; max one row per `(UserId, WorkflowId)` |
| **Agent asset** (`WorkflowComponent`) | Yes | **No** | Binary toggle; max one row per `(UserId, ComponentId)` |

**Do not build:** ratings (1–5), reviews, comments, like/dislike, download history tables.

**Download definition (workflows only):** increment **only** when the user clicks **Copy clone command** on the workflow detail page. Opening the detail page, viewing the clone box, or copying anything else does **not** count.

**Download / install button (MVP UI):** do **not** ship an active primary “Install workflow” or “Download workflow” button. If the design reference or layout includes one, keep it **disabled** (or stubbed): clicking it shows an **info toast** only — e.g. *“Just clone the GitHub repo using the command below — that's enough for MVP.”* No API call, no counter increment.

**Copy clone command flow:** existing detail UI already has **Copy clone command** — that button stays the **only** download trigger. On click: copy to clipboard (existing behavior) **and** call backend to increment `DownloadCount`.

**Naming:** rename existing `Workflow.InstallCount` → `DownloadCount` (same semantics, clearer marketplace language).

---

## Current Codebase Baseline

| Today | Change |
|---|---|
| `Workflow.InstallCount` | Rename → `DownloadCount`; wire to `POST /download` on **copy clone** only |
| No star tables | Add `WorkflowStar`, `WorkflowComponentStar` |
| `WorkflowComponent` has no counters | Add `StarCount` only |
| Card DTOs: `installCount` | → `downloadCount`, add `starCount`, `isStarred` |
| Detail page: `copyClone()` local only | On copy success → call `POST /download`; update displayed count |
| Detail page: no install/download CTA | Any install/download button **disabled** + info toast only |
| Agent asset popular sort uses parent `Workflow.InstallCount` | Change to `WorkflowComponent.StarCount` |
| Workflow trending sort uses `InstallCount` | Change to `DownloadCount` (optionally blend `StarCount` later) |

**Important:** Agent assets are stored as `workflow_components` — there is no separate `AgentAssets` table. Star FK targets `WorkflowComponent.Id`. API routes stay under `/api/agent-assets/{id}/star` for product consistency.

---

## File Structure

### Backend — create

```
backend/src/WorkflowHub.Data/Entities/
  WorkflowStar.cs
  WorkflowComponentStar.cs

backend/src/WorkflowHub.Data/Persistence/Configurations/
  WorkflowStarConfiguration.cs
  WorkflowComponentStarConfiguration.cs

backend/src/WorkflowHub.Application/Features/Workflows/
  Commands/StarWorkflow/StarWorkflowCommand.cs
  Commands/StarWorkflow/StarWorkflowCommandHandler.cs
  Commands/UnstarWorkflow/UnstarWorkflowCommand.cs
  Commands/UnstarWorkflow/UnstarWorkflowCommandHandler.cs
  Commands/RecordWorkflowDownload/RecordWorkflowDownloadCommand.cs
  Commands/RecordWorkflowDownload/RecordWorkflowDownloadCommandHandler.cs
  ReadModels/StarActionResultDto.cs
  ReadModels/RecordWorkflowDownloadResultDto.cs

backend/src/WorkflowHub.Application/Features/AgentAssets/
  Commands/StarAgentAsset/StarAgentAssetCommand.cs
  Commands/StarAgentAsset/StarAgentAssetCommandHandler.cs
  Commands/UnstarAgentAsset/UnstarAgentAssetCommand.cs
  Commands/UnstarAgentAsset/UnstarAgentAssetCommandHandler.cs

backend/src/WorkflowHub.Data/Abstractions/Repositories/
  IWorkflowStarRepository.cs          (optional thin repo — or inline in handler per existing publish pattern)

backend/src/WorkflowHub.Data/Migrations/
  <timestamp>_AddStarsAndDownloadCount.cs
```

### Backend — modify

```
backend/src/WorkflowHub.Data/Entities/Workflow.cs
backend/src/WorkflowHub.Data/Entities/WorkflowComponent.cs
backend/src/WorkflowHub.Data/Persistence/AppDbContext.cs
backend/src/WorkflowHub.Data/Persistence/Configurations/WorkflowConfiguration.cs
backend/src/WorkflowHub.Data/Persistence/Configurations/WorkflowComponentConfiguration.cs
backend/src/WorkflowHub.Data/Abstractions/Repositories/IWorkflowRepository.cs
backend/src/WorkflowHub.Data/Persistence/Repositories/WorkflowRepository.cs

backend/src/WorkflowHub.Application/Features/Discover/ReadModels/DiscoverReadModels.cs
backend/src/WorkflowHub.Application/Features/Discover/Queries/SearchWorkflows/SearchWorkflowsQuery.cs
backend/src/WorkflowHub.Application/Features/Discover/Queries/SearchWorkflows/SearchWorkflowsQueryHandler.cs
backend/src/WorkflowHub.Application/Features/Discover/Queries/GetWorkflowDetail/GetWorkflowDetailQuery.cs
backend/src/WorkflowHub.Application/Features/Discover/Queries/GetWorkflowDetail/GetWorkflowDetailQueryHandler.cs
backend/src/WorkflowHub.Application/Features/AgentAssets/ReadModels/AgentAssetReadModels.cs
backend/src/WorkflowHub.Application/Features/AgentAssets/Queries/SearchAgentAssets/SearchAgentAssetsQuery.cs
backend/src/WorkflowHub.Application/Features/AgentAssets/Queries/SearchAgentAssets/SearchAgentAssetsQueryHandler.cs
backend/src/WorkflowHub.Application/Features/MyWorkflows/ReadModels/MyWorkflowsReadModels.cs
backend/src/WorkflowHub.Application/Features/MyWorkflows/Queries/GetMyWorkflows/GetMyWorkflowsQueryHandler.cs
backend/src/WorkflowHub.Application/Features/Publish/Services/PublishWorkflowMapper.cs

backend/src/WorkflowHub.Api/Controllers/WorkflowsController.cs
backend/src/WorkflowHub.Api/Controllers/AgentAssetsController.cs

backend/src/WorkflowHub.Common/Errors/Errors.cs

workflowHub-prd-v2.md
docs/modules/discover-module.md
docs/modules/agent-assets-module.md
docs/modules/my-workflows-module.md
```

### Frontend — create

```
frontend/src/app/features/workflows/shared/models/engagement.model.ts
  StarActionResult, RecordWorkflowDownloadResult interfaces

frontend/src/app/features/workflows/shared/services/workflow-engagement-api.service.ts
  star(), unstar(), recordDownload() for workflows

frontend/src/app/features/workflows/agent-assets/services/agent-asset-engagement-api.service.ts
  star(), unstar() only

frontend/src/app/shared/ui/star-toggle/wh-star-toggle.component.ts
  Presentational: filled/outline star, count label, disabled/loading inputs
```

### Frontend — modify

```
frontend/src/app/features/workflows/discover/models/discover.model.ts
frontend/src/app/features/workflows/discover/services/discover-api.service.ts
frontend/src/app/features/workflows/discover/components/workflow-card/*
frontend/src/app/features/workflows/discover/pages/workflow-detail-page/*
frontend/src/app/features/workflows/discover/pages/discover-page/*   (trending label copy)

frontend/src/app/features/workflows/agent-assets/models/agent-asset.model.ts
frontend/src/app/features/workflows/agent-assets/components/agent-asset-card/*
frontend/src/app/features/workflows/agent-assets/pages/agent-assets-page/*  (popular sort copy)

frontend/src/app/features/workflows/my-workflows/models/my-workflows.model.ts
frontend/src/app/features/workflows/my-workflows/components/my-workflow-row/*
frontend/src/app/features/workflows/my-workflows/pages/my-workflows-page/*
```

---

## Data Model

### `workflows` table changes

```sql
ALTER TABLE workflows RENAME COLUMN "InstallCount" TO "DownloadCount";
ALTER TABLE workflows ADD COLUMN "StarCount" integer NOT NULL DEFAULT 0;
```

### `workflow_components` table changes

```sql
ALTER TABLE workflow_components ADD COLUMN "StarCount" integer NOT NULL DEFAULT 0;
-- NO DownloadCount column
```

### `workflow_stars`

```csharp
public sealed class WorkflowStar
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

EF config:
- Table: `workflow_stars`
- Unique index: `(UserId, WorkflowId)`
- FK `WorkflowId` → `workflows` ON DELETE CASCADE
- FK `UserId` → `users` ON DELETE CASCADE

### `workflow_component_stars`

```csharp
public sealed class WorkflowComponentStar
{
    public Guid Id { get; set; }
    public Guid WorkflowComponentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
```

EF config:
- Table: `workflow_component_stars`
- Unique index: `(UserId, WorkflowComponentId)`
- FK `WorkflowComponentId` → `workflow_components` ON DELETE CASCADE

---

## API Surface

### Shared response — star actions

```csharp
public sealed record StarActionResultDto(bool IsStarred, int StarCount);
```

### Workflows

| Method | Path | Behavior |
|---|---|---|
| `POST` | `/api/workflows/{id}/star` | Insert star if absent; increment `StarCount`; return `StarActionResultDto` |
| `DELETE` | `/api/workflows/{id}/star` | Delete star if present; decrement `StarCount` (floor 0); return `StarActionResultDto` |
| `POST` | `/api/workflows/{id}/download` | Increment `DownloadCount`; return updated count (called from **Copy clone command** only) |

```csharp
public sealed record RecordWorkflowDownloadResultDto(
    Guid WorkflowId,
    int DownloadCount);
```

**Idempotency:** starring twice = no-op (still returns `isStarred: true`). Unstar when not starred = no-op. **Each copy-clone click increments** `DownloadCount` by 1 (user explicitly took the workflow — simple MVP rule).

### Agent assets

| Method | Path | Behavior |
|---|---|---|
| `POST` | `/api/agent-assets/{id}/star` | Star component |
| `DELETE` | `/api/agent-assets/{id}/star` | Unstar component |

No `/download` or `/install` route for agent assets in MVP.

### Read DTO additions

**WorkflowCardDto / WorkflowDetailDto / MyWorkflowListItemDto:**

```csharp
int StarCount,
int DownloadCount,   // was InstallCount
bool IsStarred       // false when anonymous — always authenticated in MVP
```

**AgentAssetCardDto:**

```csharp
int StarCount,
bool IsStarred
// no DownloadCount
```

### Query handler changes

Pass `Guid? CurrentUserId` into search/detail queries (from controller via `ICurrentUserAccessor`). Left join star table to compute `IsStarred` per row for that user. Do **not** add N+1 loops — use projection/subquery pattern:

```csharp
.IsStarred = currentUserId != null && w.Stars.Any(s => s.UserId == currentUserId)
```

For list endpoints at scale, prefer a single query with `GroupJoin` or filtered `Any` in projection (EF translates efficiently with proper indexes).

---

## Task 1: Database Migration & Entities

**Files:**
- Create: `WorkflowStar.cs`, `WorkflowComponentStar.cs`, both configurations
- Modify: `Workflow.cs`, `WorkflowComponent.cs`, `AppDbContext.cs`, `WorkflowConfiguration.cs`, `WorkflowComponentConfiguration.cs`

- [ ] **Step 1: Add entity classes**

```csharp
// WorkflowStar.cs — full class shown in Data Model section
// WorkflowComponentStar.cs — full class shown in Data Model section
```

- [ ] **Step 2: Update Workflow entity**

```csharp
public int StarCount { get; set; }
public int DownloadCount { get; set; }  // rename property from InstallCount
```

- [ ] **Step 3: Update WorkflowComponent entity**

```csharp
public int StarCount { get; set; }
```

- [ ] **Step 4: Register DbSets + configurations with unique indexes**

- [ ] **Step 5: Generate migration**

Run:
```bash
cd backend
dotnet ef migrations add AddStarsAndDownloadCount \
  --project src/WorkflowHub.Data \
  --startup-project src/WorkflowHub.Api
```

Expected: migration renames `InstallCount` → `DownloadCount`, adds `StarCount` columns, creates both star tables with unique constraints.

- [ ] **Step 6: Apply migration locally**

```bash
dotnet ef database update --project src/WorkflowHub.Data --startup-project src/WorkflowHub.Api
```

- [ ] **Step 7: Commit**

```bash
git add backend/src/WorkflowHub.Data
git commit -m "feat(data): add workflow and agent asset stars with download rename"
```

---

## Task 2: Star / Unstar / Record Download Commands (Workflows)

**Files:**
- Create: `StarWorkflowCommand` + handler, `UnstarWorkflowCommand` + handler, `RecordWorkflowDownloadCommand` + handler, DTOs
- Modify: `IWorkflowRepository.cs`, `WorkflowRepository.cs`, `Errors.cs`

- [ ] **Step 1: Add errors**

```csharp
public static readonly ErrorDefinition AlreadyStarred = new()
{
    Code = "ALREADY_STARRED",
    MessageTemplate = "Workflow is already starred.",
    StatusCode = 409  // only if you choose strict mode; prefer idempotent no-op instead
};
public static readonly ErrorDefinition NotStarred = new() { /* ... */ };
```

**Decision:** use **idempotent** handlers (no 409 on double-star) — simpler UX, matches toggle buttons.

- [ ] **Step 2: StarWorkflowCommandHandler (transactional)**

```csharp
public sealed record StarWorkflowCommand(Guid UserId, Guid WorkflowId) : ICommand<StarActionResultDto>, ITransactionalCommand;

// Handler outline:
// 1. Load workflow (NotFound if missing)
// 2. If star row exists → return current counts
// 3. Insert WorkflowStar, increment workflow.StarCount
// 4. SaveChanges, return new StarActionResultDto(true, workflow.StarCount)
```

- [ ] **Step 3: UnstarWorkflowCommandHandler**

Mirror: delete row if exists, decrement `StarCount` with `Math.Max(0, count - 1)`.

- [ ] **Step 4: RecordWorkflowDownloadCommandHandler**

Called **only** from frontend after user clicks **Copy clone command**. Clone text stays client-built — backend only bumps the counter.

```csharp
public sealed record RecordWorkflowDownloadCommand(Guid UserId, Guid WorkflowId)
    : ICommand<RecordWorkflowDownloadResultDto>, ITransactionalCommand;

// 1. Load workflow (NotFound if missing)
// 2. workflow.DownloadCount++
// 3. SaveChanges
// 4. Return new RecordWorkflowDownloadResultDto(workflow.Id, workflow.DownloadCount)
```

- [ ] **Step 5: Wire controller endpoints on WorkflowsController**

```csharp
[HttpPost("{workflowId:guid}/star")]
[HttpDelete("{workflowId:guid}/star")]
[HttpPost("{workflowId:guid}/download")]   // not /install — copy-clone trigger only
```

- [ ] **Step 6: Build**

```bash
cd backend && dotnet build
```

Expected: BUILD SUCCESS

- [ ] **Step 7: Commit**

```bash
git commit -m "feat(api): workflow star, unstar, and record-download commands"
```

---

## Task 3: Star / Unstar Commands (Agent Assets)

**Files:**
- Create: `StarAgentAssetCommand` + handler, `UnstarAgentAssetCommand` + handler
- Modify: `AgentAssetsController.cs`

- [ ] **Step 1: Handlers target `WorkflowComponents` by id**

Validate component exists; update `WorkflowComponent.StarCount` + insert/delete `WorkflowComponentStar`.

- [ ] **Step 2: Controller routes**

```csharp
[HttpPost("{agentAssetId:guid}/star")]
[HttpDelete("{agentAssetId:guid}/star")]
```

- [ ] **Step 3: Build + commit**

```bash
cd backend && dotnet build
git commit -m "feat(api): agent asset star and unstar commands"
```

---

## Task 4: Read Path — DTOs & Query Updates

**Files:**
- Modify: all read models/handlers listed in File Structure

- [ ] **Step 1: Rename `InstallCount` → `DownloadCount` in all DTOs and handlers**

Files to grep:
```bash
rg InstallCount backend/src
```

Replace in: `DiscoverReadModels.cs`, `MyWorkflowsReadModels.cs`, `AgentAssetReadModels.cs`, all query handlers, `PublishWorkflowMapper.cs` (`DownloadCount = 0`).

- [ ] **Step 2: Add `StarCount` + `IsStarred` to workflow DTOs**

Update `SearchWorkflowsQuery` to accept optional `Guid? UserId` (passed from controller).

Handler projection example:

```csharp
new WorkflowCardDto(
    w.Id,
    w.Name,
    // ...existing fields...
    w.StarCount,
    w.DownloadCount,
    currentUserId != null && dbContext.WorkflowStars.Any(s => s.WorkflowId == w.Id && s.UserId == currentUserId),
    matchedComponent)
```

Prefer including `WorkflowStars` filter in the main query projection to avoid N+1 — refactor to a single `Select` projection.

- [ ] **Step 3: Add `StarCount` + `IsStarred` to `AgentAssetCardDto`**

Update `SearchAgentAssetsQuery` + handler; change popular sort:

```csharp
// was: OrderByDescending(c => c.Workflow.InstallCount)
OrderByDescending(c => c.StarCount).ThenByDescending(c => c.Workflow.UpdatedAtUtc)
```

- [ ] **Step 4: Update `GetWorkflowDetailQueryHandler`**

Return `starCount`, `downloadCount`, `isStarred` on `WorkflowDetailDto`.

- [ ] **Step 5: Build + commit**

```bash
cd backend && dotnet build
git commit -m "feat(api): expose star and download fields on read DTOs"
```

---

## Task 5: Frontend Models & API Services

**Files:**
- Create: `engagement.model.ts`, `workflow-engagement-api.service.ts`, `agent-asset-engagement-api.service.ts`
- Modify: `discover.model.ts`, `agent-asset.model.ts`, `my-workflows.model.ts`

- [ ] **Step 1: Shared types**

```typescript
export interface StarActionResult {
  isStarred: boolean;
  starCount: number;
}

export interface RecordWorkflowDownloadResult {
  workflowId: string;
  downloadCount: number;
}
```

- [ ] **Step 2: Workflow engagement API**

```typescript
@Injectable({ providedIn: 'root' })
export class WorkflowEngagementApiService {
  star(workflowId: string): Observable<StarActionResult> { /* POST .../star */ }
  unstar(workflowId: string): Observable<StarActionResult> { /* DELETE .../star */ }
  recordDownload(workflowId: string): Observable<RecordWorkflowDownloadResult> { /* POST .../download */ }
}
```

- [ ] **Step 3: Agent asset engagement API (star only)**

- [ ] **Step 4: Update card/detail models**

Replace `installCount` with `downloadCount`; add `starCount`, `isStarred`.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/app/features/workflows
git commit -m "feat(frontend): engagement API services and model fields"
```

---

## Task 6: Shared Star Toggle Component

**Files:**
- Create: `frontend/src/app/shared/ui/star-toggle/wh-star-toggle.component.ts`

- [ ] **Step 1: Presentational component (no HTTP, no Router)**

```typescript
@Component({
  selector: 'wh-star-toggle',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  template: `
    <button type="button" class="star-btn" [class.on]="starred()" [disabled]="disabled()" (click)="toggle.emit()">
      <wh-icon [name]="starred() ? 'star' : 'star'" [size]="16" />
      <span>{{ count() }}</span>
    </button>
  `
})
export class WhStarToggleComponent {
  readonly starred = input(false);
  readonly count = input(0);
  readonly disabled = input(false);
  readonly toggle = output<void>();
}
```

Enhance filled vs outline star via CSS class on button (icon stays `star`; use `fill-current` when starred).

- [ ] **Step 2: Commit**

---

## Task 7: Workflow Card & Detail UI

**Files:**
- Modify: `workflow-card.component.*`, `workflow-detail-page.component.*`

- [ ] **Step 1: Card layout (design reference)**

```text
⭐ 120    ⬇ 3.2k
```

Use compact number formatting helper:

```typescript
export function formatCount(n: number): string {
  if (n >= 1000) return `${(n / 1000).toFixed(1).replace(/\.0$/, '')}k`;
  return String(n);
}
```

Place in `frontend/src/app/shared/utils/format-count.ts`.

- [ ] **Step 2: Card star toggle**

Page or card host calls `WorkflowEngagementApiService` — **presentational card** receives `starred`, `starCount`, `downloadCount` as inputs and emits `starToggle` output; page handles API + optimistic update.

- [ ] **Step 3: Detail page — copy clone is the only download trigger**

Extend existing `copyClone()` in `workflow-detail-page.component.ts`:

```typescript
copyClone(): void {
  const command = this.cloneCommand();
  if (!command) return;

  void navigator.clipboard.writeText(command).then(() => {
    this.copied.set(true);
    this.engagement.recordDownload(this.workflowId()).subscribe({
      next: (result) => {
        this.workflow.update(w => w ? { ...w, downloadCount: result.downloadCount } : w);
      },
      error: (err) => this.notifications.error(toUserFacingError(err))
    });
    this.notifications.success('Clone command copied.');
  });
}
```

**Do not** call `recordDownload` on page load or when user only selects text in the clone box.

- [ ] **Step 4: Detail page — disabled install/download button (stub)**

If layout includes a primary “Install workflow” / “Download workflow” button (design reference or future slot):

```html
<button type="button" class="btn btn-outline btn-sm btn-block" disabled (click)="onDownloadStubClick()">
  Install workflow
</button>
```

```typescript
onDownloadStubClick(): void {
  this.notifications.info('Just clone the GitHub repo using the command below — that\'s enough for MVP.');
}
```

Button stays **disabled** so accidental clicks are rare; `(click)` on disabled buttons may not fire in all browsers — prefer **enabled-looking stub** with `aria-disabled` + click handler only, or a non-disabled outline button that never calls the API:

```html
<button type="button" class="btn btn-outline btn-sm btn-block" (click)="onDownloadStubClick()">
  Install workflow
</button>
```

Use the **non-disabled stub + info toast** pattern so the message always shows. Label can read “Install workflow (coming soon)” if helpful.

- [ ] **Step 5: Detail star toggle** — same pattern as card.

- [ ] **Step 6: Rename sidebar stat “Installs” → “Downloads”**

- [ ] **Step 7: Build**

```bash
cd frontend && npm run build
```

- [ ] **Step 8: Commit**

```bash
git commit -m "feat(ui): workflow stars and copy-clone download counting"
```

---

## Task 8: Agent Asset Card UI (Stars Only)

**Files:**
- Modify: `agent-asset-card.component.*`, `agent-assets-page.component.*`

- [ ] **Step 1: Show ⭐ count on asset card; no download icon**

- [ ] **Step 2: Star toggle on card** (stop row click propagation on star button)

- [ ] **Step 3: Update popular section subtitle** — “by stars” not “by parent workflow installs”

- [ ] **Step 4: Build + commit**

---

## Task 9: My Workflows & Discover Copy Updates

**Files:**
- Modify: `my-workflow-row`, `my-workflows-page`, discover trending labels

- [ ] **Step 1: Rename “installs” → “downloads” in UI strings**

- [ ] **Step 2: Add star count to my-workflow row meta (optional per design — at minimum show downloads correctly)**

- [ ] **Step 3: My workflows stats strip**

```text
Published | Total downloads | Files indexed
```

Sum `downloadCount` instead of `installCount`.

- [ ] **Step 4: Commit**

---

## Task 10: PRD & Module Docs

**Files:**
- Modify: `workflowHub-prd-v2.md`, `docs/modules/*.md`

- [ ] **Step 1: PRD §2.2** — replace install_count note with stars + download_count (copy-clone trigger)

- [ ] **Step 2: PRD §5.1 data model** — add star tables + counters

- [ ] **Step 3: PRD §6 API** — document star routes + `POST /workflows/{id}/download` (not install)

- [ ] **Step 4: PRD §4.4** — clarify GEN-3: download increments on **Copy clone command**, not page view; no separate install flow in MVP UI

- [ ] **Step 4: Module docs** — card fields, engagement rules, agent assets stars-only

- [ ] **Step 5: Commit**

```bash
git commit -m "docs: stars and downloads marketplace rules"
```

---

## Task 11: Manual Verification Checklist

- [ ] Star workflow from discover card → count increments, icon filled, refresh persists
- [ ] Unstar → count decrements, icon outline
- [ ] Double-star does not double count
- [ ] Open workflow detail **without** copying clone → download count unchanged
- [ ] Click **Copy clone command** → clipboard updated, download count +1, toast shown
- [ ] Click stub **Install workflow** button (if shown) → info toast only, **no** download increment
- [ ] Star agent asset from agent assets page → count increments; no download UI anywhere on agent assets
- [ ] My workflows shows download totals correctly
- [ ] `dotnet build` and `npm run build` green

---

## Self-Review (spec coverage)

| Requirement | Task |
|---|---|
| WorkflowStars table + unique constraint | Task 1 |
| AgentAssetStars (`workflow_component_stars`) | Task 1 |
| Denormalized StarCount / DownloadCount | Task 1, 2, 3 |
| No download table | Task 1 (explicitly omitted) |
| POST/DELETE star APIs | Task 2, 3 |
| POST download API (workflows only, copy-clone trigger) | Task 2 |
| Card DTO with isStarred | Task 4 |
| Agent assets stars only (no downloads) | Task 3, 8 |
| Binary star (not rating) | All star handlers idempotent toggle |
| Do not count page views as downloads | Task 7 |
| Install/download button stub → toast only, no API | Task 7 |
| Ranking hooks for later | Task 4 notes (sort by DownloadCount / StarCount) |

**Gap vs user's generic spec:** agent asset `/install` intentionally **excluded** (“stars only”). Workflow counting uses **`POST /download`** on **Copy clone command** only — no active install button in MVP UI.

---

## Execution Order Recommendation

1. Task 1 (schema) — blocks everything  
2. Tasks 2–3 (write commands) — can parallelize  
3. Task 4 (read paths)  
4. Tasks 5–9 (frontend)  
5. Task 10 (docs)  
6. Task 11 (verification)

Estimated: **2–3 focused sessions** if executed inline; **4–6 tasks** if subagent-driven with review between tasks.
