import { WhIconName } from '../../../../shared/ui/icon/wh-icon.component';

export type WorkflowsNavId = 'discover' | 'search' | 'mine' | 'create' | 'settings';

export interface WorkflowsNavItem {
  id: WorkflowsNavId;
  icon: WhIconName;
  label: string;
  path: string;
  /** Shown in mobile tab bar */
  tabBar?: boolean;
  /** Primary nav row (sidebar / topbar) */
  primary?: boolean;
}

export const WORKFLOWS_REGISTRY_NAV: WorkflowsNavItem[] = [
  {
    id: 'discover',
    icon: 'compass',
    label: 'Discover',
    path: '/workflows/discover',
    tabBar: true,
    primary: true
  },
  {
    id: 'search',
    icon: 'search',
    label: 'Search',
    path: '/workflows/search',
    tabBar: true,
    primary: true
  },
  {
    id: 'mine',
    icon: 'folder-git-2',
    label: 'My workflows',
    path: '/workflows/mine',
    tabBar: true,
    primary: true
  }
];

export const WORKFLOWS_TAB_NAV: WorkflowsNavItem[] = [
  ...WORKFLOWS_REGISTRY_NAV,
  { id: 'create', icon: 'plus', label: 'Create', path: '/workflows/create', tabBar: true },
  { id: 'settings', icon: 'settings', label: 'Settings', path: '/workflows/settings', tabBar: true }
];
