export type ComponentType = 'rule' | 'command' | 'hook' | 'skill' | 'subagent';

export interface AnalysisCapability {
  name: string;
  description: string;
}

export interface AnalysisComponent {
  path: string;
  componentType: ComponentType;
  title: string;
  summary: string;
  capabilities: AnalysisCapability[];
  keywords: string[];
  searchPhrases: string[];
  technologies: string[];
  dependencies: string[];
}

export interface AnalysisOutput {
  workflowName: string;
  description: string;
  tags: string[];
  suggestedDependencies: string[];
  components: AnalysisComponent[];
}

export type DependencyKind = 'mcp' | 'plugin';
export type DependencyRequirement = 'required' | 'optional';

export interface WorkflowDependency {
  kind: DependencyKind;
  name: string;
  requirement: DependencyRequirement;
  note: string;
}

export type SetupComplexity = 'beginner' | 'intermediate' | 'advanced';

export type TargetAudience =
  | 'frontend'
  | 'backend'
  | 'fullstack'
  | 'devops'
  | 'ai-engineering'
  | 'general';

export interface RepoValidationResult {
  exists: boolean;
  isPublic: boolean;
  accessible: boolean;
}

export interface NameAvailabilityResult {
  available: boolean;
  suggestion?: string;
}

export interface PublishWorkflowRequest {
  repositoryUrl: string;
  builtForAgents: string[];
  sourceIde?: string;
  workflowName: string;
  description: string;
  tags: string[];
  dependencies: WorkflowDependency[];
  complexity: SetupComplexity;
  targetAudience: TargetAudience;
  analysis: AnalysisOutput;
}

export interface PublishWorkflowResponse {
  id: string;
  workflowName: string;
  workflowCode: string;
}
