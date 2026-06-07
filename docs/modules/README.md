# Module Docs

One doc per application module: what it is, what it does, the screens and forms it contains, the
data it shows, the API it needs, and the data-model changes it implies. These are the design specs
we agree on **before** building each module, so frontend (Angular) and backend (.NET) stay aligned.

> Source of truth for overall product scope: [`../../workflowHub-prd-v2.md`](../../workflowHub-prd-v2.md).
> Coding standards for *how* to build any of this: `../../../ai-rules/` (see `RULES-INDEX.md`).  
> **Folder layout backlog (confusions + refactor checklist):** [`../frontend-folder-structure-alignment.md`](../frontend-folder-structure-alignment.md).

## Modules

| Module | Status | Doc |
|---|---|---|
| **Settings** (profile & account — user details) | 🟢 Built (MVP UI) | [`settings-module.md`](settings-module.md) |
| **Discover** (workflow marketplace — browse / search / detail) | 🟡 In progress | [`discover-module.md`](discover-module.md) |
| **Agent Assets** (AI agent asset marketplace) | 🟡 In progress | [`agent-assets-module.md`](agent-assets-module.md) |
| **Learn** (knowledge center — guides & articles) | 🟢 Built (MVP) | [`learn-module.md`](learn-module.md) |
| **My workflows** (author dashboard) | 🟢 Built (MVP) | [`my-workflows-module.md`](my-workflows-module.md) |
| Auth (Google login + JWT) | ⚪ Not started | _tbd_ |
| **Publish** (wizard — repo + AI analysis → workflow) | 🟢 Built (MVP) | [`publish-module.md`](publish-module.md) |
| Install / Convert | ⚪ Not started | _tbd_ |

Legend: ⚪ not started · 🟡 spec drafted · 🟢 built
