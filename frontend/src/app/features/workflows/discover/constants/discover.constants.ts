import { ComponentType } from '../../publish/models/publish-analysis.model';
import { COMPONENT_TYPE_LABELS, COMPONENT_TYPES } from '../../publish/constants/publish.constants';
import { InstallLevel, InstallTargetAgent } from '../models/discover.model';

export const DISCOVER_PAGE_SIZE = 12;
export const DISCOVER_TRENDING_COUNT = 3;
export const DISCOVER_SECTION_COUNT = 3;

export const DISCOVER_COMPONENT_TYPES = COMPONENT_TYPES;
export const DISCOVER_COMPONENT_TYPE_LABELS = COMPONENT_TYPE_LABELS;

export const SOURCE_IDE_LABELS: Record<string, string> = {
  cursor: 'Cursor',
  claude: 'Claude',
  copilot: 'Copilot'
};

export const INSTALL_TARGET_AGENT_OPTIONS: { id: InstallTargetAgent; label: string }[] = [
  { id: 'same', label: 'Same as original' },
  { id: 'cursor', label: 'Cursor' },
  { id: 'claude', label: 'Claude' },
  { id: 'copilot', label: 'Copilot' }
];

export const INSTALL_LEVEL_OPTIONS: { id: InstallLevel; label: string; hint: string }[] = [
  { id: 'project', label: 'Project level', hint: 'Install into the current workspace' },
  { id: 'system', label: 'System level', hint: 'Install into global agent config' }
];

export type DiscoverComponentType = ComponentType;
