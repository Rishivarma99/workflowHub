import { ComponentType } from '../../publish/models/publish-analysis.model';
import { COMPONENT_TYPE_LABELS, COMPONENT_TYPES } from '../../publish/constants/publish.constants';

export const AGENT_ASSETS_PAGE_SIZE = 12;
export const AGENT_ASSETS_SECTION_COUNT = 6;

export const AGENT_ASSET_TYPES = COMPONENT_TYPES;
export const AGENT_ASSET_TYPE_LABELS: Record<ComponentType, string> = {
  ...COMPONENT_TYPE_LABELS,
  subagent: 'Agents'
};

export const AGENT_ASSET_FILTER_TYPES = ['rule', 'command', 'skill', 'hook', 'subagent'] as const;

export type AgentAssetFilterType = (typeof AGENT_ASSET_FILTER_TYPES)[number];
