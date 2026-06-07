import { ComponentType, SetupComplexity, TargetAudience } from '../models/publish-analysis.model';

export const PUBLISH_WIZARD_STEPS = ['Repository', 'Analysis', 'Metadata', 'Review'] as const;

export const SOURCE_IDE_OPTIONS = [
  { id: 'cursor', label: 'Cursor' },
  { id: 'claude', label: 'Claude' },
  { id: 'copilot', label: 'Copilot' }
] as const;

export const COMPONENT_TYPES: ComponentType[] = [
  'rule',
  'command',
  'hook',
  'skill',
  'subagent'
];

export const COMPONENT_TYPE_LABELS: Record<ComponentType, string> = {
  rule: 'Rule',
  command: 'Command',
  hook: 'Hook',
  skill: 'Skill',
  subagent: 'Subagent'
};

export const COMPLEXITY_OPTIONS: { id: SetupComplexity; label: string }[] = [
  { id: 'beginner', label: 'Beginner' },
  { id: 'intermediate', label: 'Intermediate' },
  { id: 'advanced', label: 'Advanced' }
];

export const AUDIENCE_OPTIONS: { id: TargetAudience; label: string }[] = [
  { id: 'frontend', label: 'Frontend' },
  { id: 'backend', label: 'Backend' },
  { id: 'fullstack', label: 'Full Stack' },
  { id: 'devops', label: 'DevOps' },
  { id: 'ai-engineering', label: 'AI Engineering' },
  { id: 'general', label: 'General' }
];

export const GITHUB_REPO_PATTERN = /^https?:\/\/github\.com\/[\w.-]+\/[\w.-]+\/?$/i;
