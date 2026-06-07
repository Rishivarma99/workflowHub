/** Canonical analysis prompt — keep in sync with docs/prompts/repository-analysis-prompt.md */
const ANALYSIS_PROMPT_TEMPLATE = `You are analyzing a workflow repository for publication in WorkflowHub.

Repository URL:
{{REPOSITORY_URL}}

Your task is to analyze the entire repository and generate a JSON document describing the workflow and all searchable workflow components.

The goal is to maximize discoverability and search quality.

---

## Important Rules

### 1. Analyze the Entire Repository

Review all relevant workflow files and understand:

* What the workflow does
* What problems it solves
* What capabilities it provides
* What technologies it uses
* What integrations it depends on

Do not focus only on filenames.

---

### 2. Supported Component Types

Every component must be classified as one of:

* rule
* command
* hook
* skill
* subagent

Only include components that belong to one of these types.

Do not include README files, documentation, examples, screenshots, assets, build scripts, or package manager files unless they are clearly part of the workflow itself.

---

### 3. Discoverability First

Users search for capabilities, use cases, outcomes, and technologies — not filenames.

---

### 4. Extract All Capabilities

Include every meaningful capability a component supports, not just the primary one.

---

### 5. Generate Real Search Phrases

Generate natural-language phrases users would realistically type (e.g. "implement google login", "setup microsoft oauth").

---

### 6. Technologies

Extract all relevant technologies (frameworks, languages, services, integrations).

---

### 7. Dependencies

Identify external dependencies: MCP servers, plugins, APIs, databases, external services.

---

### 8. Workflow Naming

Generate a marketplace-friendly name — clear, descriptive, and unique when possible. Avoid generic names like "Workflow" or "Automation Workflow".

---

## Output Schema

Return valid JSON only.

\`\`\`json
{
  "workflowName": "",
  "description": "",
  "tags": [],
  "suggestedDependencies": [],
  "components": [
    {
      "path": "",
      "componentType": "",
      "title": "",
      "summary": "",
      "capabilities": [{ "name": "", "description": "" }],
      "keywords": [],
      "searchPhrases": [],
      "technologies": [],
      "dependencies": []
    }
  ]
}
\`\`\`

Before generating output ensure every component has a valid componentType, meaningful capabilities, realistic searchPhrases, extracted technologies and dependencies, and that the output is valid JSON with no explanatory text outside the JSON.`;

export function buildAnalysisPrompt(repositoryUrl: string): string {
  return ANALYSIS_PROMPT_TEMPLATE.replace('{{REPOSITORY_URL}}', repositoryUrl.trim());
}
