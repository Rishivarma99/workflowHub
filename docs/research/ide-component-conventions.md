# IDE Component Conventions — Cursor vs Claude Code vs GitHub Copilot

**Purpose:** Workflow Hub indexes and installs workflow *components* (skills, rules, subagents, commands, hooks). To detect them at publish and place them correctly at install, we need to know **where each tool stores each component and how it names them.** This doc captures that, current as of **June 2026**.

> TL;DR — every tool uses the same shape: a hidden config folder (`.cursor/`, `.claude/`, `.github/`) containing **Markdown files with YAML frontmatter**, one per component, grouped into per-type subfolders. Hooks are the exception (JSON/settings, not Markdown). We can detect components from **folder + filename + frontmatter**.

---

## 1. Cursor (`.cursor/`)

Cursor 2.4 (Jan 2026) added subagents + a skills marketplace, so all five component types now exist.

| Component | Folder | File / naming | Format |
|---|---|---|---|
| **Rules** | `.cursor/rules/` | `<name>.mdc` (new rules created as a folder with `RULE.md`); also repo-root `AGENTS.md` | Markdown + frontmatter: `description`, `globs`, `alwaysApply` |
| **Commands** | `.cursor/commands/` | `<name>.md` | Markdown prompt, invoked by typing `/<name>` |
| **Hooks** | `.cursor/hooks.json` **+** `.cursor/hooks/` | config JSON declares events; each entry's `command` points to a **script file** in `.cursor/hooks/` (e.g. `start-frontend.sh`) | JSON config + executable scripts (`.sh`, `.ts`, …) |
| **Subagents** | `.cursor/agents/` | `<name>.md` | Markdown + frontmatter |
| **Skills** | `.cursor/skills/<name>/` (also `.agents/skills/`) | `SKILL.md` per folder; walked recursively; nested folders scope the skill to that subtree | Markdown + frontmatter; optional `scripts/`, `references/`, `assets/` |

Notes:
- Plain `.md` in `.cursor/rules/` is **ignored** — must be `.mdc` (or `AGENTS.md`).
- Skills can live in **any** `.cursor/skills/` folder in the tree (monorepo-friendly); nested = directory-scoped.
- **A Cursor hook is two files, not one.** `hooks.json` is only the registration; the real logic is a separate script in `.cursor/hooks/` referenced by the `command` path. Example:
  ```jsonc
  // .cursor/hooks.json
  { "version": 1,
    "hooks": { "sessionStart": [ { "command": ".cursor/hooks/start-frontend.sh", "timeout": 30 } ] } }
  ```
  ```
  .cursor/hooks/start-frontend.sh   <- the actual executable the hook runs
  ```
  → To carry a hook we must bundle **both** the JSON entry **and** the script file(s) it points to.

---

## 2. Claude Code (`.claude/`)

| Component | Folder | File / naming | Format |
|---|---|---|---|
| **Rules / memory** | repo root or `.claude/` | `CLAUDE.md` (project), `~/.claude/CLAUDE.md` (user); some setups use `.claude/rules/` for path-scoped rules | Markdown |
| **Commands** | `.claude/commands/` (project), `~/.claude/commands/` (user) | `<name>.md` → `/<name>` | Markdown + frontmatter |
| **Skills** | `.claude/skills/<name>/` | `SKILL.md` (required) + optional `scripts/`, `references/`, `assets/` | Markdown + frontmatter |
| **Subagents** | `.claude/agents/` (project), `~/.claude/agents/` (user) | `<name>.md` | Markdown + frontmatter: `name`, `description`, `tools`, `model` |
| **Hooks** | `.claude/settings.json` (project), `~/.claude/settings.json` (user) | inside settings JSON | JSON keyed by lifecycle event (`PreToolUse`, `UserPromptSubmit`, … 25 events) |
| **MCP / plugins** | repo root / `.claude/` | `.mcp.json` | JSON: MCP server config |

Notes:
- **Commands and skills are effectively merged:** `.claude/commands/deploy.md` and `.claude/skills/deploy/SKILL.md` both produce `/deploy`. Skills add bundled scripts/assets; commands are a single prompt file.
- Subagents invoked via `@agent-name`; described in frontmatter so Claude auto-delegates.

---

## 3. GitHub Copilot (`.github/`)

Copilot uses different vocabulary; the mapping to our types matters.

| Our type | Copilot name | Folder | File / naming | Format |
|---|---|---|---|---|
| **Rules** | Custom instructions | `.github/` and `.github/instructions/` | `copilot-instructions.md` (repo-wide); `<name>.instructions.md` (scoped) | Markdown + frontmatter `applyTo` (glob) |
| **Commands** | Prompt files | `.github/prompts/` | `<name>.prompt.md` → `/<name>` | Markdown + frontmatter: `description`, `model`, `tools`, `agent` |
| **Subagents** | Chat modes / Custom agents | `.github/chatmodes/` or `.github/agents/` | `<name>.chatmode.md` or `<name>.agent.md` | Markdown + frontmatter |
| **Skills** | (limited / emerging) | — | no stable first-class equivalent yet | — |
| **Hooks** | (none) | — | no direct equivalent | — |

Notes:
- Copilot has **no hooks** and **no first-class skills** comparable to Cursor/Claude as of now — relevant for install/convert (can't always round-trip these types).

---

## 4. Cross-tool cheat sheet (by our `component_type`)

| Our `component_type` | Cursor | Claude Code | Copilot |
|---|---|---|---|
| `rule` | `.cursor/rules/*.mdc` | `CLAUDE.md` / `.claude/rules/` | `.github/copilot-instructions.md`, `.github/instructions/*.instructions.md` |
| `command` | `.cursor/commands/*.md` | `.claude/commands/*.md` | `.github/prompts/*.prompt.md` |
| `skill` | `.cursor/skills/<n>/SKILL.md` | `.claude/skills/<n>/SKILL.md` | — (emerging) |
| `subagent` | `.cursor/agents/*.md` | `.claude/agents/*.md` | `.github/chatmodes/*.chatmode.md`, `.github/agents/*.agent.md` |
| `hook` | `.cursor/hooks.json` + `.cursor/hooks/*.sh` (config + scripts) | `.claude/settings.json` (hooks block) + script files | — (none) |
| `mcp_plugin` | MCP config | `.mcp.json` | — |

---

## 5. Implications for Workflow Hub

**Detection at publish (§4.3 parser):**
- Match by **folder convention first** (`*/rules/*.mdc`, `*/commands/*.md`, `*/skills/**/SKILL.md`, `*/agents/*.md`, `hooks.json`/settings hooks block), then confirm with **frontmatter / file shape**.
- `SKILL.md` is the strongest skill signal across both Cursor and Claude — identical convention.
- `.mdc` extension uniquely signals a Cursor rule.

**Install / convert mapping (§4.4 `target_mapping`):**
- Most types map 1:1 between Cursor ↔ Claude (rules, commands, skills, subagents share Markdown+frontmatter shape; just relocate folders).
- **Hooks differ structurally and are multi-file:** Cursor = `hooks.json` registration **+** referenced scripts in `.cursor/hooks/`; Claude = a block inside `settings.json` **+** its scripts; Copilot = none. A hook component must capture the config entry **and** every script its `command` points to — detection (§4.3) should follow the `command` path and pull those files into the same component record. Converting hooks needs real transformation (config shape differs), not just a folder move.
- **Copilot is lossy:** no hooks, no first-class skills — surface a clear "not supported in target" notice rather than silently dropping.

**Naming normalization:** strip extensions per target (`.mdc` ↔ `.md`), preserve the component's base `<name>`, and rewrite frontmatter keys where they differ (e.g. Copilot `applyTo` ↔ Cursor `globs`).

---

## Sources
- [Cursor — Rules](https://cursor.com/docs/context/rules)
- [Cursor — Commands](https://cursor.com/docs/context/commands)
- [Cursor — Hooks](https://cursor.com/docs/hooks)
- [Cursor — Agent Skills](https://cursor.com/docs/skills)
- [Cursor — Subagents](https://cursor.com/docs/context/subagents)
- [Cursor 2.4 changelog](https://cursor.com/changelog/2-4)
- [Claude Code — Subagents](https://code.claude.com/docs/en/sub-agents)
- [Claude Code — Skills](https://code.claude.com/docs/en/skills)
- [VS Code — Custom instructions for Copilot](https://code.visualstudio.com/docs/copilot/customization/custom-instructions)
- [Microsoft — Prompt files and instructions files explained](https://devblogs.microsoft.com/dotnet/prompt-files-and-instructions-files-explained/)
- [GitHub Docs — Add custom instructions for Copilot CLI](https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions)
