# Module Docs

One doc per application module: what it is, what it does, the screens and forms it contains, the
data it shows, the API it needs, and the data-model changes it implies. These are the design specs
we agree on **before** building each module, so frontend (Angular) and backend (.NET) stay aligned.

> Source of truth for overall product scope: [`../../workflowHub-prd-v2.md`](../../workflowHub-prd-v2.md).
> Coding standards for *how* to build any of this: `../../../ai-rules/` (see `RULES-INDEX.md`).

## Modules

| Module | Status | Doc |
|---|---|---|
| **Settings** (incl. User Management) | 🟡 Spec drafted | [`settings-module.md`](settings-module.md) |
| **Discover** (browse / search / detail) | 🟡 Decisions landing | [`discover-module.md`](discover-module.md) |
| Auth (Google login + JWT) | ⚪ Not started | _tbd_ |
| Publish a workflow | ⚪ Not started | _tbd_ |
| Install / Convert | ⚪ Not started | _tbd_ |

Legend: ⚪ not started · 🟡 spec drafted · 🟢 built
