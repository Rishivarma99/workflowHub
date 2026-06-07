import { COMPONENT_TYPES } from '../constants/publish.constants';
import {
  AnalysisComponent,
  AnalysisOutput,
  ComponentType
} from '../models/publish-analysis.model';

export interface AnalysisValidationResult {
  ok: true;
  data: AnalysisOutput;
}

export interface AnalysisValidationError {
  ok: false;
  errors: string[];
}

export type ParseAnalysisResult = AnalysisValidationResult | AnalysisValidationError;

function isNonEmptyString(value: unknown): value is string {
  return typeof value === 'string' && value.trim().length > 0;
}

function isStringArray(value: unknown): value is string[] {
  return Array.isArray(value) && value.every((item) => typeof item === 'string');
}

function validateComponent(raw: unknown, index: number, errors: string[]): AnalysisComponent | null {
  if (!raw || typeof raw !== 'object') {
    errors.push(`components[${index}]: must be an object`);
    return null;
  }

  const c = raw as Record<string, unknown>;

  if (!isNonEmptyString(c['path'])) {
    errors.push(`components[${index}].path: required`);
  }
  if (!COMPONENT_TYPES.includes(c['componentType'] as ComponentType)) {
    errors.push(
      `components[${index}].componentType: must be one of ${COMPONENT_TYPES.join(', ')}`
    );
  }
  if (!isNonEmptyString(c['title'])) {
    errors.push(`components[${index}].title: required`);
  }
  if (!isNonEmptyString(c['summary'])) {
    errors.push(`components[${index}].summary: required`);
  }

  const capabilities = c['capabilities'];
  if (!Array.isArray(capabilities) || capabilities.length === 0) {
    errors.push(`components[${index}].capabilities: at least one capability required`);
  } else {
    capabilities.forEach((cap, ci) => {
      if (!cap || typeof cap !== 'object') {
        errors.push(`components[${index}].capabilities[${ci}]: must be an object`);
        return;
      }
      const capObj = cap as Record<string, unknown>;
      if (!isNonEmptyString(capObj['name'])) {
        errors.push(`components[${index}].capabilities[${ci}].name: required`);
      }
      if (!isNonEmptyString(capObj['description'])) {
        errors.push(`components[${index}].capabilities[${ci}].description: required`);
      }
    });
  }

  if (!isStringArray(c['keywords']) || c['keywords'].length === 0) {
    errors.push(`components[${index}].keywords: at least one keyword required`);
  }
  if (!isStringArray(c['searchPhrases']) || c['searchPhrases'].length === 0) {
    errors.push(`components[${index}].searchPhrases: at least one search phrase required`);
  }
  if (!isStringArray(c['technologies'])) {
    errors.push(`components[${index}].technologies: must be an array of strings`);
  }
  if (!isStringArray(c['dependencies'])) {
    errors.push(`components[${index}].dependencies: must be an array of strings`);
  }

  if (errors.some((e) => e.startsWith(`components[${index}]`))) {
    return null;
  }

  return {
    path: (c['path'] as string).trim(),
    componentType: c['componentType'] as ComponentType,
    title: (c['title'] as string).trim(),
    summary: (c['summary'] as string).trim(),
    capabilities: (capabilities as { name: string; description: string }[]).map((cap) => ({
      name: cap.name.trim(),
      description: cap.description.trim()
    })),
    keywords: (c['keywords'] as string[]).map((k) => k.trim()).filter(Boolean),
    searchPhrases: (c['searchPhrases'] as string[]).map((p) => p.trim()).filter(Boolean),
    technologies: ((c['technologies'] as string[]) ?? []).map((t) => t.trim()).filter(Boolean),
    dependencies: ((c['dependencies'] as string[]) ?? []).map((d) => d.trim()).filter(Boolean)
  };
}

export function parseAndValidateAnalysisJson(raw: string): ParseAnalysisResult {
  const errors: string[] = [];
  let parsed: unknown;

  try {
    parsed = JSON.parse(raw.trim());
  } catch {
    return { ok: false, errors: ['Invalid JSON — check syntax and try again.'] };
  }

  if (!parsed || typeof parsed !== 'object') {
    return { ok: false, errors: ['Root value must be a JSON object.'] };
  }

  const root = parsed as Record<string, unknown>;

  if (!isNonEmptyString(root['workflowName'])) {
    errors.push('workflowName: required');
  }
  if (!isNonEmptyString(root['description'])) {
    errors.push('description: required');
  }
  if (!isStringArray(root['tags']) || root['tags'].length === 0) {
    errors.push('tags: at least one tag required');
  }
  if (!isStringArray(root['suggestedDependencies'])) {
    errors.push('suggestedDependencies: must be an array of strings');
  }

  const componentsRaw = root['components'];
  if (!Array.isArray(componentsRaw) || componentsRaw.length === 0) {
    errors.push('components: at least one component required');
  }

  const components: AnalysisComponent[] = [];
  if (Array.isArray(componentsRaw)) {
    componentsRaw.forEach((item, index) => {
      const component = validateComponent(item, index, errors);
      if (component) {
        components.push(component);
      }
    });
  }

  if (errors.length > 0) {
    return { ok: false, errors };
  }

  return {
    ok: true,
    data: {
      workflowName: (root['workflowName'] as string).trim(),
      description: (root['description'] as string).trim(),
      tags: (root['tags'] as string[]).map((t) => t.trim().toLowerCase()).filter(Boolean),
      suggestedDependencies: (root['suggestedDependencies'] as string[])
        .map((d) => d.trim())
        .filter(Boolean),
      components
    }
  };
}
