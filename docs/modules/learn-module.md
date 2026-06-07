# Learn Module

> Educate users about workflows, agent assets, design principles, and platform concepts.

**Status:** 🟢 Built (MVP)  
**PRD:** [`workflowHub-prd-v2.md`](../../workflowHub-prd-v2.md) §4.8  
**Routes:** `/workflows/learn`, `/workflows/learn/:slug`

---

## Purpose

Learn is WorkflowHub's **knowledge center** — not a blog. It mixes documentation, guides, and educational articles to help users understand the platform and become better creators.

Learn search is **fully isolated**: it returns articles only, never workflows or agent assets.

---

## Content types

| Type | Character | Examples (seed) |
|---|---|---|
| **Guide** | Evergreen, may be pinned, rarely changes | How to Create High Quality Workflows, Understanding Workflows |
| **Article** | Published over time, educational | Building a Research Workflow, Common Workflow Mistakes |

---

## Screens

### Learn homepage (`/workflows/learn`)

| Section | Content |
|---|---|
| Hero | Search Learn… (URL-driven `q`, `page`, `category`) |
| Getting Started | 3–5 pinned guides |
| Latest Articles | Recent published articles (non-pinned) |
| Categories | Workflow Design, Agent Assets, Architecture, Tutorials, Best Practices |

### Article detail (`/workflows/learn/:slug`)

- Category badge, title, summary, author, published date
- Markdown body
- Back to Learn

---

## API

| Method | Path | Purpose |
|---|---|---|
| `GET` | `/api/articles/home` | Pinned guides, latest articles, categories |
| `POST` | `/api/articles/search` | Paginated article search |
| `GET` | `/api/articles/{slug}` | Article detail (markdown content) |

Search request shape matches other marketplaces: `{ query, page, pageSize, filters?: { category } }`.

---

## Data model

```
ArticleCategory
  id, name, slug, description

Article
  id, slug, title, summary, content (markdown)
  category_id, author_id
  status (draft | published)
  content_type (guide | article)
  is_pinned, published_at
  search_text (denormalized)
  + ITrackable, ISoftDeletable
```

---

## Roles (MVP)

| Role | Capabilities |
|---|---|
| **User** | Read, search, share articles |
| **Super Admin** | Create/edit/delete/publish/pin (via API + seed; admin UI post-MVP) |

Super Admin is determined by `SuperAdmin:Emails` in appsettings until a full role system exists.

---

## Seeded pinned guides (MVP)

1. How to Create High Quality Workflows
2. Understanding Workflows
3. Understanding Agent Assets
4. Publishing Guidelines
