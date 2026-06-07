import { PagedResult } from '../../../../shared/types/api';

export interface WorkflowAuthor {
  name: string;
  displayName?: string | null;
  avatarUrl?: string | null;
}

export interface MatchedComponent {
  componentId: string;
  componentType: string;
  path: string;
  title: string;
  capabilityName: string;
}

export interface WorkflowCard {
  id: string;
  name: string;
  description: string;
  tags: string[];
  workflowCode: string;
  builtForAgents: string[];
  sourceIde: string;
  starCount: number;
  downloadCount: number;
  isStarred: boolean;
  componentTypes: string[];
  componentCounts: Record<string, number>;
  author: WorkflowAuthor;
  matchedComponent?: MatchedComponent | null;
}

export interface WorkflowSearchCriteria {
  query?: string;
  page: number;
  pageSize: number;
  sortBy?: 'relevance' | 'downloads' | 'createdAt';
  filters: Record<string, string | number | boolean | undefined>;
}

export type WorkflowSearchResult = PagedResult<WorkflowCard>;

export interface DiscoverHome {
  trending: WorkflowCard[];
  recent: WorkflowCard[];
}

export interface WorkflowBrowseCriteria {
  page: number;
  pageSize: number;
  types?: string[];
}

export interface WorkflowCapability {
  name: string;
  description: string;
}

export interface WorkflowComponentDetail {
  id: string;
  path: string;
  gitHubUrl: string;
  componentType: string;
  title: string;
  summary: string;
  starCount: number;
  isStarred: boolean;
  capabilities: WorkflowCapability[];
  keywords: string[];
  searchPhrases: string[];
  technologies: string[];
  dependencies: string[];
}

export interface WorkflowDependency {
  kind: string;
  name: string;
  requirement: string;
  note?: string | null;
}

export interface WorkflowDetail {
  id: string;
  name: string;
  description: string;
  tags: string[];
  repositoryUrl: string;
  commitSha: string;
  workflowCode: string;
  builtForAgents: string[];
  sourceIde: string;
  complexity: string;
  targetAudience: string;
  starCount: number;
  downloadCount: number;
  isStarred: boolean;
  updatedAtUtc: string;
  author: WorkflowAuthor;
  dependencies: WorkflowDependency[];
  components: WorkflowComponentDetail[];
}

export type InstallTargetAgent = 'same' | 'cursor' | 'claude' | 'copilot';
export type InstallLevel = 'project' | 'system';

export interface GenerateInstallPromptRequest {
  targetAgent: InstallTargetAgent;
  installLevel: InstallLevel;
}

export interface GenerateInstallPromptResponse {
  prompt: string;
}
