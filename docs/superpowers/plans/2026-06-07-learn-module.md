# Learn Module Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship WorkflowHub's Learn knowledge center — isolated article search, homepage (pinned guides + latest articles + categories), and markdown article detail pages — seeded with four pinned onboarding guides.

**Architecture:** Third independent search domain (`POST /api/articles/search`) with `Article` + `ArticleCategory` tables, markdown `Content`, denormalized `SearchText`, and idempotent dev seeding after migrations. Frontend mirrors Discover's hero-card + URL-driven search (`q`, `page`, `category`) under `/workflows/learn`.

**Tech Stack:** .NET 8 + EF Core + CQRS, PostgreSQL ILike search, Angular 20 standalone + signals, inline `wh-markdown` renderer (no new npm deps).

**Rules to load before coding:**
- Backend always-on: `backend/01-architecture/backend-architecture-overview.md`, `core-type-rules.md`, `cqrs-overview.md`
- Backend tasks: `query-rules.md`, `entity-design-and-navigation-rules.md`, `validation-and-ef-configuration-rules.md`, `dbcontext-rules.md`, `seeding-rules.md`, `api-layer-rules.md`, `api-response-contract-rules.md`, `authorization-rules.md`
- Cross-cutting: `cross-cutting/search-architecture-rules.md`
- Frontend always-on: `frontend/01-architecture/folder-structure-and-boundary-rules.md`, `naming-and-file-rules.md`, `forbidden-patterns-rules.md`
- Frontend tasks: `03-data/api-and-http-rules.md`, `02-angular/component-and-template-rules.md`, `02-angular/signals-and-rxjs-rules.md`, `03-data/error-handling-rules.md`, `04-ui-styling/scss-and-design-token-rules.md`

---

## Product Rules (locked for MVP)

| Rule | Detail |
|---|---|
| **Scope** | Learn search returns **articles only** — never workflows or agent assets |
| **Content types** | `Guide` (evergreen, may be pinned) and `Article` (published over time) |
| **Pinned guides (seed)** | How to Create High Quality Workflows, Understanding Workflows, Understanding Agent Assets, Publishing Guidelines |
| **Content format** | Markdown stored in DB; rendered client-side |
| **Auth** | All Learn endpoints login-gated (consistent with Discover) |
| **Admin (MVP)** | Super Admin via `SuperAdmin:Emails` appsettings allowlist; seed content at startup; admin CRUD UI deferred |
| **URLs** | `/workflows/learn`, `/workflows/learn/:slug` |

---

## File Structure

### Backend — create

```
backend/src/WorkflowHub.Data/Entities/
  Article.cs
  ArticleCategory.cs
  ArticleStatus.cs
  ArticleContentType.cs

backend/src/WorkflowHub.Data/Persistence/Configurations/
  ArticleConfiguration.cs
  ArticleCategoryConfiguration.cs

backend/src/WorkflowHub.Data/Persistence/Seeding/
  LearnContentSeedService.cs

backend/src/WorkflowHub.Application/Features/Learn/
  ReadModels/ArticleCardDto.cs
  ReadModels/ArticleDetailDto.cs
  ReadModels/ArticleCategoryDto.cs
  ReadModels/LearnHomeDto.cs
  Services/LearnSearchHelper.cs
  Queries/GetLearnHome/GetLearnHomeQuery.cs
  Queries/GetLearnHome/GetLearnHomeQueryHandler.cs
  Queries/SearchArticles/SearchArticlesQuery.cs
  Queries/SearchArticles/SearchArticlesQueryHandler.cs
  Queries/GetArticleBySlug/GetArticleBySlugQuery.cs
  Queries/GetArticleBySlug/GetArticleBySlugQueryHandler.cs

backend/src/WorkflowHub.Api/Contracts/Requests/Learn/
  SearchArticlesRequest.cs

backend/src/WorkflowHub.Api/Controllers/
  ArticlesController.cs
```

### Backend — modify

```
backend/src/WorkflowHub.Data/Persistence/AppDbContext.cs
backend/src/WorkflowHub.Data/Bootstrap/DatabaseExtensions.cs
backend/src/WorkflowHub.Api/appsettings.json
backend/src/WorkflowHub.Api/appsettings.Development.json
workflowHub-prd-v2.md
docs/modules/README.md
docs/modules/learn-module.md
```

### Frontend — create

```
frontend/src/app/features/workflows/learn/
  models/learn.model.ts
  services/learn-api.service.ts
  components/article-card/article-card.component.{ts,html,scss}
  components/wh-markdown/wh-markdown.component.{ts,scss}
  pages/learn-page/learn-page.component.{ts,html,scss}
  pages/article-detail-page/article-detail-page.component.{ts,html,scss}
  styles/learn-shared.scss
```

### Frontend — modify

```
frontend/src/app/features/workflows/workflows.routes.ts
frontend/src/app/features/workflows/layout/sidebar/workflows-nav.config.ts
frontend/src/app/shared/ui/icon/wh-icon.component.ts
```

---

### Task 1: Data model + migration

**Files:**
- Create entity + configuration files listed above
- Modify `AppDbContext.cs`

- [ ] **Step 1:** Add `ArticleCategory` and `Article` entities with `ITrackable` + `ISoftDeletable` on `Article`
- [ ] **Step 2:** Configure unique slug indexes, FK to category + author, global soft-delete filter
- [ ] **Step 3:** Run `dotnet ef migrations add AddLearnArticles --project src/WorkflowHub.Data --startup-project src/WorkflowHub.Api`
- [ ] **Step 4:** Apply migration via dev startup

---

### Task 2: Seed pinned guides + categories

**Files:**
- Create `LearnContentSeedService.cs`
- Modify `DatabaseExtensions.cs`

- [ ] **Step 1:** Seed five categories (Workflow Design, Agent Assets, Architecture, Tutorials, Best Practices)
- [ ] **Step 2:** Ensure system author user exists (`learn@workflowhub.local`)
- [ ] **Step 3:** Seed four pinned published guides with markdown content + 3 sample articles
- [ ] **Step 4:** Idempotent — skip if categories already exist

---

### Task 3: CQRS queries + API

**Files:**
- Create Learn feature query/handler files + `ArticlesController.cs`

- [ ] **Step 1:** `GetLearnHome` — pinned guides (max 5), latest 6 articles, all categories
- [ ] **Step 2:** `SearchArticles` — tokenized ILike on SearchText/title/summary; optional category slug filter; published only
- [ ] **Step 3:** `GetArticleBySlug` — full markdown detail; 404 when draft/missing
- [ ] **Step 4:** Wire `ArticlesController` at `api/articles` with `[Authorize]`

---

### Task 4: Frontend Learn homepage

**Files:**
- Create learn page, article card, API service, models

- [ ] **Step 1:** Add nav item + routes (`learn` before `:workflowId`)
- [ ] **Step 2:** Hero search form; URL params `q`, `page`, `category`
- [ ] **Step 3:** Idle sections: Getting Started (pinned), Latest Articles, Categories
- [ ] **Step 4:** Search mode: paginated article cards only

---

### Task 5: Article detail + markdown renderer

**Files:**
- Create `wh-markdown`, `article-detail-page`

- [ ] **Step 1:** Fetch by slug; show category badge, meta, markdown body
- [ ] **Step 2:** Back link to Learn; share-friendly title layout
- [ ] **Step 3:** Minimal typography matching discover detail patterns

---

### Task 6: Docs + verification

- [ ] **Step 1:** Update PRD §4.8, §5.1, §5.3, §6, §10
- [ ] **Step 2:** Add `docs/modules/learn-module.md`
- [ ] **Step 3:** Run `dotnet build` and `npm run build`

---

## Self-Review

| Spec requirement | Task |
|---|---|
| Isolated Learn search | Task 3 `SearchArticles` |
| Pinned onboarding guides | Task 2 seed |
| Guides vs Articles content types | Task 1 enums + card badges |
| Markdown content | Task 1 + Task 5 |
| Super Admin seed (MVP) | Task 2 + appsettings allowlist reserved |
| `/learn/:slug` URLs | Task 4 routes |
| Separate from Workflows/Agent Assets | Task 3 query scope |
