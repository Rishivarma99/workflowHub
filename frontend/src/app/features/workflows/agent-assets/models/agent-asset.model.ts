import { PagedResult } from '../../../../shared/types/api';

export interface AgentAssetAuthor {
  name: string;
  displayName?: string | null;
  avatarUrl?: string | null;
}

export interface AgentAssetCard {
  id: string;
  name: string;
  description: string;
  assetType: string;
  path: string;
  workflowId: string;
  workflowName: string;
  sourceIde: string;
  starCount: number;
  isStarred: boolean;
  author: AgentAssetAuthor;
}

export interface AgentAssetSearchCriteria {
  query?: string;
  page: number;
  pageSize: number;
  sortBy?: 'relevance' | 'stars' | 'downloads' | 'createdAt';
  filters: Record<string, string | number | boolean | undefined>;
}

export type AgentAssetSearchResult = PagedResult<AgentAssetCard>;

export interface AgentAssetsHome {
  popular: AgentAssetCard[];
  recent: AgentAssetCard[];
}

export interface AgentAssetBrowseCriteria {
  page: number;
  pageSize: number;
  types?: string[];
}
