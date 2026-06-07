namespace WorkflowHub.Application.Features.Install.Services;

public static class InstallPromptTemplate
{
    public const string Template = """
        # WorkflowHub Installation Prompt

        You are installing a WorkflowHub workflow.

        ## Workflow Information

        Workflow Name:
        {{WorkflowName}}

        Workflow Code:
        {{WorkflowCode}}

        Repository:
        {{RepositoryUrl}}

        Pinned Commit:
        {{CommitSha}}

        Built For Agent:
        {{BuiltForAgent}}

        Target Agent:
        {{TargetAgent}}

        Installation Level:
        {{InstallLevel}}

        ---

        ## Workflow Assets vs Workflow Outputs

        These are two completely different things. Do not confuse them.

        ### Workflow Assets (installed now)

        Assets are copied from the cloned repository into the user's target project or system location.

        Examples:
        - Commands
        - Rules
        - Skills
        - Hooks
        - Subagents
        - MCP configs
        - Plugins

        ### Workflow Outputs (generated later)

        Outputs are created when the user runs the workflow after installation.

        Examples:
        - PR review reports
        - Documentation
        - Architecture analysis
        - Requirements specs
        - Test plans
        - Code files

        Outputs must NEVER be written into the clone folder (for example `~/Desktop/WorkflowHub/{{WorkflowCode}}`).
        That folder is a temporary installation source only.

        ---

        ## Output Destination Rule (Critical)

        All workflow outputs, generated files, reports, code changes, documentation, analyses, and artifacts must be created inside the target project selected by the user (or the target system location when Installation Level is System Level).

        The cloned workflow repository is a temporary installation source only and must never be used as the destination for workflow outputs unless explicitly requested by the user.

        Example:
        - Workflow installed into: `D:/Projects/MyApi`
        - User runs: `/generate-docs`
        - Output goes to: `D:/Projects/MyApi/docs/` or `D:/Projects/MyApi/README.md`
        - Output must NOT go to: `~/Desktop/WorkflowHub/{{WorkflowCode}}/docs`

        ---

        ## Step 1 - Gather Required Inputs

        Ask the user for the following information before performing any installation.

        ### Installation Target

        If Installation Level is Project Level:

        1. Detect the current workspace/project path.
        2. Display the detected path.
        3. Ask the user to:
           - Press Enter to use the detected path.
           - Or provide another project path.

        If Installation Level is System Level:

        1. Detect the global installation path used by the target agent.
        2. Display the detected path.
        3. Ask the user to:
           - Press Enter to use the detected path.
           - Or provide another path.

        ### Clone Location

        Ask:

        Where should I clone the workflow repository?

        Default:
        ~/Desktop/WorkflowHub

        Allow the user to accept the default or provide another location.

        Remind the user: the clone location is only a temporary source for installation. It is not where workflow outputs belong.

        Do not continue until all required inputs are collected.

        ---

        ## Step 2 - Clone Repository and Analyze Contents

        Clone the workflow repository at the pinned commit.

        Analyze the repository contents.

        ### 2a - Identify Workflow Assets (to install)

        Identify assets that will be copied into the target project or system location:
        - Rules
        - Commands
        - Skills
        - Hooks
        - Subagents
        - Agents
        - MCP configurations
        - Plugins
        - Templates
        - Any other AI-agent-related assets

        ### 2b - Identify Expected Workflow Outputs (generated later)

        After analyzing assets, identify what this workflow produces when the user runs it:
        - Expected Output Files
        - Expected Output Folders
        - Expected Artifacts

        Infer these from commands, skills, rules, README files, and workflow documentation in the repository.

        If outputs are unclear, ask the user to confirm expected output locations before continuing.

        Document expected outputs for WORKFLOWS.md in Step 7.

        ---

        ## Step 3 - Determine Current Target Agent Structure

        Do not assume folder names or installation locations.

        Determine the latest recommended structure and conventions used by the target agent.

        Use the target agent's currently supported mechanisms.

        ---

        ## Step 4 - Convert Assets

        If the source agent and target agent differ:

        1. Analyze the intent and behavior of each asset.
        2. Find the closest equivalent in the target agent.
        3. Preserve behavior whenever possible.
        4. Follow the target agent's latest conventions.

        If an exact equivalent does not exist:
        - Explain the limitation.
        - Describe the closest alternative.
        - Ask for confirmation if significant behavior changes are required.

        ---

        ## Step 5 - Resolve Naming Conflicts

        Before installing any file:

        Check whether an equivalent file already exists.

        Default behavior:

        Do not overwrite.

        Create a renamed copy using:

        {OriginalFileName}-whb-{{WorkflowCodeSuffix}}

        Example:

        review-command.md

        becomes

        review-command-whb-{{WorkflowCodeSuffixLower}}

        If conflicts exist:

        Offer:
        1. Create renamed copy (Default)
        2. Replace existing file
        3. Skip installation

        ---

        ## Step 6 - Install Assets

        Install the converted assets using the target agent's latest supported structure and conventions.

        Install workflow assets only. Do not create workflow outputs during installation unless the repository explicitly contains starter/template files meant to be copied into the target project.

        Verify successful installation.

        ---

        ## Step 7 - Update Documentation

        Maintain a single file:

        WORKFLOWS.md

        Create it if it does not exist.

        Do not create workflow-specific documentation files.

        Add or update a section for this workflow.

        For each workflow include:
        - Workflow Name
        - Workflow Code
        - Purpose
        - How To Use
        - Output Location (where generated files go when the user runs the workflow)
        - Generated Files (specific files or folders produced at runtime; use "None" if the workflow produces no files)
        - Expected Artifacts (reports, analyses, comments, or other non-file outputs)
        - Required MCPs
        - Required Plugins
        - Installed Assets
        - Skipped Assets

        Example — PR Review Workflow:

        ```markdown
        ## PR Review Workflow

        Purpose
        Reviews pull requests.

        Output Location
        Current project workspace.

        Generated Files
        None.

        Expected Artifacts
        Produces review comments only.
        ```

        Example — Documentation Workflow:

        ```markdown
        ## Documentation Workflow

        Purpose
        Generate project documentation.

        Output Location
        docs/

        Generated Files
        architecture.md
        api-spec.md
        ```

        Keep the documentation concise and practical.

        The purpose of WORKFLOWS.md is to help the user understand:
        - What gets installed (assets)
        - What gets generated later (outputs)

        ---

        ## Step 8 - Generate Installation Summary

        After installation, provide a summary.

        Include:
        - Workflow Name
        - Workflow Code
        - Source Agent
        - Target Agent
        - Installation Level
        - Installation Target (project or system path)
        - Clone Location (source only — not for outputs)
        - Installed Assets
        - Skipped Assets
        - Expected Output Location
        - Expected Generated Files / Artifacts
        - Documentation Updated

        Clearly identify any limitations or unsupported features.

        Remind the user: when they run this workflow, all outputs must be written to the installation target — never to the clone folder.

        Installation is not complete until the summary is shown.
        """;
}
