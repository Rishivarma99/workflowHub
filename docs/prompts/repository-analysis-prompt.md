# Repository Analysis Prompt

Copy this prompt into your AI agent. Replace `{{REPOSITORY_URL}}` with the repository URL from Step 1.

---

You are analyzing a workflow repository for publication in WorkflowHub.

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

Do not include:

* README files
* Documentation files
* Example files
* Screenshots
* Assets
* Build scripts
* Package manager files

unless they are clearly part of the workflow itself.

---

### 3. Discoverability First

The output will power search.

Users search for:

* capabilities
* use cases
* outcomes
* technologies

Users do NOT search for filenames.

Bad:

```text
authentication.md
```

Good:

```text
google login
microsoft login
oauth implementation
jwt authentication
```

---

### 4. Extract All Capabilities

A component may support multiple capabilities.

Example:

A single authentication skill may support:

* Google Login
* Microsoft Login
* GitHub Login
* OAuth
* JWT Authentication
* Refresh Tokens

Include ALL capabilities.

Do not include only the primary capability.

---

### 5. Generate Real Search Phrases

Generate search phrases that users would realistically type.

Examples:

* implement google login
* setup microsoft oauth
* add github authentication
* create pull request review
* generate staged file report
* automate commit message generation

Search phrases should be natural language.

---

### 6. Technologies

Extract all relevant technologies.

Examples:

* React
* Next.js
* ASP.NET Core
* PostgreSQL
* MongoDB
* GitHub
* Azure
* AWS
* Docker
* Kubernetes
* OAuth

---

### 7. Dependencies

Identify external dependencies.

Examples:

* MCP servers
* Plugins
* APIs
* Databases
* External services

These should be listed so users know what is required before installation.

---

### 8. Workflow Naming

Generate a marketplace-friendly workflow name.

Requirements:

* Clear
* Descriptive
* Unique when possible
* Prefer including repository context
* Avoid generic names such as:

  * Workflow
  * Automation Workflow
  * AI Workflow

---

## Output Schema

Return valid JSON only.

```json
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

      "capabilities": [
        {
          "name": "",
          "description": ""
        }
      ],

      "keywords": [],

      "searchPhrases": [],

      "technologies": [],

      "dependencies": []
    }
  ]
}
```

---

## Field Definitions

### workflowName

Generate a marketplace-friendly name for the workflow.

Example:

```text
Rishi React Authentication Workflow
```

---

### description

1-3 sentences explaining:

* what the workflow does
* who it is for
* what problems it solves

---

### tags

Generate 5-10 useful tags.

Examples:

```text
authentication
oauth
react
aspnet-core
github
automation
```

---

### suggestedDependencies

Workflow-level dependencies.

Examples:

```text
GitHub MCP
PostgreSQL MCP
Slack Plugin
```

---

### path

Repository-relative file path.

Example:

```text
skills/google-login.md
```

---

### componentType

One of:

```text
rule
command
hook
skill
subagent
```

---

### title

Human-friendly component title.

Example:

```text
Google Login Implementation
```

---

### summary

1-3 sentences describing the component's purpose.

---

### capabilities

List every meaningful capability provided by the component.

Example:

```json
[
  {
    "name": "Google Login",
    "description": "Implement Google OAuth authentication"
  },
  {
    "name": "Microsoft Login",
    "description": "Implement Microsoft Entra ID authentication"
  }
]
```

---

### keywords

Short searchable keywords.

Example:

```json
[
  "google login",
  "oauth",
  "authentication",
  "react"
]
```

---

### searchPhrases

Natural language search phrases.

Example:

```json
[
  "implement google login",
  "setup google oauth",
  "add authentication to react app",
  "configure microsoft login"
]
```

---

### technologies

All technologies used or referenced.

Example:

```json
[
  "React",
  "ASP.NET Core",
  "Google OAuth"
]
```

---

### dependencies

Component-specific dependencies.

Example:

```json
[
  "GitHub MCP",
  "PostgreSQL MCP"
]
```

---

## Final Validation Checklist

Before generating output ensure:

* Every component has a valid componentType.
* Every component has meaningful capabilities.
* Every component has realistic search phrases.
* Multiple capabilities are included when supported.
* Technologies are extracted.
* Dependencies are extracted.
* Output is valid JSON.
* No explanatory text outside the JSON.
