namespace WorkflowHub.Data.Persistence.Seeding;

internal static class LearnSeedContent
{
    internal sealed record CategorySeed(string Name, string Slug, string Description);

    internal sealed record ArticleSeed(
        string Slug,
        string Title,
        string Summary,
        string CategorySlug,
        string ContentType,
        bool IsPinned,
        string Content);

    internal static readonly CategorySeed[] Categories =
    [
        new("Workflow Design", "workflow-design", "Principles and patterns for building effective workflows."),
        new("Agent Assets", "agent-assets", "Rules, commands, skills, hooks, and agents explained."),
        new("Architecture", "architecture", "System design patterns for AI-assisted development."),
        new("Tutorials", "tutorials", "Step-by-step walkthroughs and how-tos."),
        new("Best Practices", "best-practices", "Proven approaches and common pitfalls.")
    ];

    internal static readonly ArticleSeed[] Articles =
    [
        new(
            "how-to-create-high-quality-workflows",
            "How to Create High Quality Workflows",
            "A practical guide to designing workflows that are discoverable, reproducible, and easy to adopt.",
            "workflow-design",
            "guide",
            true,
            """
            # How to Create High Quality Workflows

            A great workflow solves a **complete problem** — not just one file in isolation. Think in outcomes: PR review, release notes, research, documentation.

            ## Start with the outcome

            Before you open your IDE, write one sentence: *What does someone get after they clone this workflow?*

            ## Structure your repo clearly

            - Keep rules, commands, skills, hooks, and agents in predictable paths
            - Add a README that explains setup in under two minutes
            - Pin dependencies (MCP servers, plugins) at publish time

            ## Write for discovery

            Use descriptive titles, tags, and summaries. Search matches your metadata **and** contained asset keywords — help both.

            ## Test before you publish

            Clone your own repo at the pinned commit. Run the workflow end-to-end in a clean environment.

            ## Ship incrementally

            Publish a focused v1. Re-publish when you improve the workflow — the commit SHA keeps installs reproducible.
            """),
        new(
            "understanding-workflows",
            "Understanding Workflows",
            "What a Workflow Hub workflow is, how pinning works, and why workflows are complete solutions.",
            "workflow-design",
            "guide",
            true,
            """
            # Understanding Workflows

            On Workflow Hub, a **workflow** is a pinned reference to a public GitHub repository at an exact commit SHA — plus searchable metadata and a manifest of indexed agent assets.

            ## Workflows are bundles

            A single workflow can contain rules, commands, skills, hooks, and subagents together. Other files (scripts, docs, configs) ship with the clone but are not separately indexed.

            ## Pinning guarantees reproducibility

            Every user gets the same commit you published. Re-publish to update the pin when you improve the workflow.

            ## Workflows vs agent assets

            - **Discover** finds complete workflows (solutions)
            - **Agent Assets** finds individual reusable building blocks

            Search domains stay separate so intent stays clear.
            """),
        new(
            "understanding-agent-assets",
            "Understanding Agent Assets",
            "The five indexed asset types — rules, commands, skills, hooks, and agents — and how they compose into workflows.",
            "agent-assets",
            "guide",
            true,
            """
            # Understanding Agent Assets

            Agent assets are the **reusable AI building blocks** inside a workflow. Workflow Hub indexes exactly five types:

            | Type | Typical use |
            |---|---|
            | **Rule** | Persistent instructions for your agent |
            | **Command** | Invocable slash commands |
            | **Skill** | Packaged capability modules |
            | **Hook** | Lifecycle automation triggers |
            | **Agent** | Subagent definitions |

            ## Assets live inside workflows

            Every indexed asset belongs to a published workflow. Star an asset to bookmark it; open it to see the parent workflow and clone the full bundle.

            ## Naming and keywords matter

            Builders search Agent Assets by name, summary, and capabilities. Write metadata as if you're helping a stranger find your file in ten seconds.
            """),
        new(
            "publishing-guidelines",
            "Publishing Guidelines",
            "Standards for publishing workflows: public repos, accurate metadata, dependency disclosure, and no secrets.",
            "best-practices",
            "guide",
            true,
            """
            # Publishing Guidelines

            Publishing makes your workflow discoverable. Follow these standards so installers trust what they clone.

            ## Repository requirements

            - Public GitHub repository only
            - Commit must exist at publish time
            - README with setup steps

            ## Metadata honesty

            Title and description should match what the workflow actually does. Tags should reflect real capabilities — not engagement bait.

            ## Declare dependencies

            List MCP servers and plugins the workflow needs. Never paste tokens, API keys, or env values.

            ## Manifest quality

            Use the provided analysis template. Accurate `component_type` values power Agent Asset search.

            ## After publish

            You can edit metadata, re-publish to bump the commit, or delete the workflow. Soft-deleted workflows disappear from search.
            """),
        new(
            "building-a-research-workflow",
            "Building a Research Workflow",
            "Patterns for workflows that gather sources, synthesize findings, and produce structured research output.",
            "tutorials",
            "article",
            false,
            """
            # Building a Research Workflow

            Research workflows combine **rules** (how to cite sources), **commands** (kick off a research pass), and sometimes **skills** (domain-specific analysis).

            ## Recommended structure

            1. A rule defining citation and uncertainty standards
            2. A command that accepts a topic and produces an outline
            3. Optional hooks to log session context

            ## Keep scope narrow

            One workflow per research style — competitive analysis vs literature review vs codebase exploration.
            """),
        new(
            "common-workflow-mistakes",
            "Common Workflow Mistakes",
            "Frequent publishing and design mistakes — and how to avoid them.",
            "best-practices",
            "article",
            false,
            """
            # Common Workflow Mistakes

            ## Vague titles

            "My Cursor Setup" doesn't help anyone. Prefer "PR Review Workflow for TypeScript Monorepos."

            ## Missing dependency disclosure

            If your workflow needs GitHub MCP or a plugin, say so before the clone command.

            ## Over-stuffed bundles

            Dozens of unrelated rules in one repo hurt discovery. Split by outcome or publish focused asset collections.

            ## Stale pins

            Re-publish when you fix breaking changes so installers aren't stuck on old commits.
            """),
        new(
            "agent-asset-best-practices",
            "Agent Asset Best Practices",
            "How to write rules, commands, and skills that other builders actually want to reuse.",
            "agent-assets",
            "article",
            false,
            """
            # Agent Asset Best Practices

            ## One responsibility per asset

            A rule that tries to do everything becomes unmaintainable. Compose small assets inside a workflow.

            ## Write actionable summaries

            Lead with *when* to use the asset and *what* it changes in the agent's behavior.

            ## Use consistent naming

            Match file paths and titles to how builders search: "code-review-rule.md" beats "stuff.md".
            """)
    ];
}
