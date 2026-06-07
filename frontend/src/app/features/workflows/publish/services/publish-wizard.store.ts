import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, filter, switchMap } from 'rxjs';
import { NotificationService } from '../../../../core/services/notification.service';
import { toUserFacingError } from '../../../../shared/utils/api/to-user-facing-error';
import { buildAnalysisPrompt } from '../constants/analysis-prompt';
import { GITHUB_REPO_PATTERN } from '../constants/publish.constants';
import {
  AnalysisOutput,
  RepoValidationResult,
  SetupComplexity,
  TargetAudience,
  WorkflowDependency
} from '../models/publish-analysis.model';
import { parseAndValidateAnalysisJson } from '../utils/validate-analysis-json';
import { PublishApiService } from './publish-api.service';

@Injectable()
export class PublishWizardStore {
  private readonly destroyRef = inject(DestroyRef);
  private readonly api = inject(PublishApiService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  readonly step = signal(0);
  readonly repositoryUrl = signal('');
  readonly builtForAgents = signal<string[]>(['cursor']);
  readonly repoValidation = signal<RepoValidationResult | null>(null);
  readonly repoValidating = signal(false);

  readonly analysisJsonRaw = signal('');
  readonly analysis = signal<AnalysisOutput | null>(null);
  readonly analysisErrors = signal<string[]>([]);

  readonly aiSuggestedName = signal('');
  readonly workflowName = signal('');
  readonly description = signal('');
  readonly tags = signal<string[]>([]);
  readonly tagDraft = signal('');

  readonly dependencies = signal<WorkflowDependency[]>([]);
  readonly runsStandalone = signal(false);
  readonly complexity = signal<SetupComplexity>('intermediate');
  readonly targetAudience = signal<TargetAudience>('general');
  readonly ownershipConfirmed = signal(false);

  readonly nameChecking = signal(false);
  readonly nameAvailable = signal<boolean | null>(null);
  readonly nameSuggestion = signal<string | null>(null);

  readonly publishing = signal(false);

  readonly analysisPrompt = computed(() => buildAnalysisPrompt(this.repositoryUrl()));

  readonly repoUrlValid = computed(() => GITHUB_REPO_PATTERN.test(this.repositoryUrl().trim()));

  readonly componentCounts = computed(() => {
    const data = this.analysis();
    if (!data) {
      return new Map<string, number>();
    }
    const counts = new Map<string, number>();
    for (const c of data.components) {
      counts.set(c.componentType, (counts.get(c.componentType) ?? 0) + 1);
    }
    return counts;
  });

  readonly canAdvanceFromRepo = computed(
    () => this.repoUrlValid() && this.builtForAgents().length > 0
  );

  readonly canAdvanceFromAnalysis = computed(
    () => this.analysis() !== null && this.analysisErrors().length === 0
  );

  readonly canAdvanceFromMetadata = computed(() => {
    const name = this.workflowName().trim();
    const desc = this.description().trim();
    const tagsOk = this.tags().length > 0;
    const nameOk = name.length >= 3 && this.nameAvailable() !== false;
    const depsOk =
      this.runsStandalone() ||
      (this.dependencies().length > 0 && this.dependencies().every((d) => d.name.trim()));
    return (
      nameOk &&
      desc.length >= 10 &&
      tagsOk &&
      depsOk &&
      this.ownershipConfirmed()
    );
  });

  constructor() {
    toObservable(this.workflowName)
      .pipe(
        debounceTime(400),
        distinctUntilChanged(),
        filter((name) => name.trim().length >= 3),
        switchMap((name) => {
          this.nameChecking.set(true);
          return this.api.checkWorkflowName(name);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          this.nameAvailable.set(result.available);
          this.nameSuggestion.set(result.suggestion ?? null);
          this.nameChecking.set(false);
        },
        error: () => {
          this.nameAvailable.set(null);
          this.nameChecking.set(false);
        }
      });
  }

  setStep(index: number): void {
    this.step.set(index);
  }

  nextStep(): void {
    const current = this.step();
    if (current === 0) {
      this.startRepoValidation();
    }
    if (current === 1 && !this.validateAnalysisPaste()) {
      return;
    }
    if (current === 2 && !this.canAdvanceFromMetadata()) {
      return;
    }
    this.step.set(Math.min(current + 1, 3));
  }

  prevStep(): void {
    this.step.set(Math.max(this.step() - 1, 0));
  }

  cancel(): void {
    void this.router.navigate(['/workflows/mine']);
  }

  onRepoUrlChange(value: string): void {
    this.repositoryUrl.set(value);
    this.repoValidation.set(null);
  }

  startRepoValidation(): void {
    const url = this.repositoryUrl().trim();
    if (!GITHUB_REPO_PATTERN.test(url)) {
      return;
    }

    this.repoValidating.set(true);
    this.api.validateRepository(url).subscribe({
      next: (result) => {
        this.repoValidation.set(result);
        this.repoValidating.set(false);
      },
      error: () => {
        this.repoValidation.set(null);
        this.repoValidating.set(false);
      }
    });
  }

  validateAnalysisPaste(): boolean {
    const result = parseAndValidateAnalysisJson(this.analysisJsonRaw());
    if (!result.ok) {
      this.analysisErrors.set(result.errors);
      this.analysis.set(null);
      return false;
    }

    this.analysisErrors.set([]);
    this.analysis.set(result.data);
    this.applyAnalysisToMetadata(result.data);
    return true;
  }

  onAnalysisJsonChange(value: string): void {
    this.analysisJsonRaw.set(value);
    if (this.analysisErrors().length > 0) {
      this.analysisErrors.set([]);
    }
  }

  useAiSuggestedName(): void {
    const suggested = this.aiSuggestedName();
    if (suggested) {
      this.workflowName.set(suggested);
    }
  }

  useNameSuggestion(): void {
    const suggestion = this.nameSuggestion();
    if (suggestion) {
      this.workflowName.set(suggestion);
    }
  }

  addTag(): void {
    const t = this.tagDraft().trim().toLowerCase().replace(/^#/, '');
    if (!t || this.tags().includes(t)) {
      this.tagDraft.set('');
      return;
    }
    this.tags.update((prev) => [...prev, t]);
    this.tagDraft.set('');
  }

  removeTag(tag: string): void {
    this.tags.update((prev) => prev.filter((t) => t !== tag));
  }

  addDependency(): void {
    this.runsStandalone.set(false);
    this.dependencies.update((prev) => [
      ...prev,
      { kind: 'mcp', name: '', requirement: 'required', note: '' }
    ]);
  }

  updateDependency(index: number, patch: Partial<WorkflowDependency>): void {
    this.dependencies.update((prev) =>
      prev.map((d, i) => (i === index ? { ...d, ...patch } : d))
    );
  }

  removeDependency(index: number): void {
    this.dependencies.update((prev) => prev.filter((_, i) => i !== index));
  }

  setStandalone(value: boolean): void {
    this.runsStandalone.set(value);
    if (value) {
      this.dependencies.set([]);
    }
  }

  async copyAnalysisPrompt(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.analysisPrompt());
      this.notifications.success('Analysis prompt copied.');
    } catch {
      this.notifications.error('Could not copy to clipboard.');
    }
  }

  publish(): void {
    const analysis = this.analysis();
    if (!analysis || this.publishing()) {
      return;
    }

    this.publishing.set(true);

    this.api
      .publishWorkflow({
        repositoryUrl: this.repositoryUrl().trim(),
        builtForAgents: this.builtForAgents(),
        workflowName: this.workflowName().trim(),
        description: this.description().trim(),
        tags: this.tags(),
        dependencies: this.runsStandalone() ? [] : this.dependencies(),
        complexity: this.complexity(),
        targetAudience: this.targetAudience(),
        analysis
      })
      .subscribe({
        next: (result) => {
          this.publishing.set(false);
          this.notifications.success(`"${result.workflowName}" published.`);
          void this.router.navigate(['/workflows/mine']);
        },
        error: (err) => {
          this.publishing.set(false);
          this.notifications.error(toUserFacingError(err));
        }
      });
  }

  builtForAgentsLabel(): string {
    const map: Record<string, string> = {
      cursor: 'Cursor',
      claude: 'Claude',
      copilot: 'Copilot'
    };
    return this.builtForAgents()
      .map((agent) => map[agent] ?? agent)
      .join(', ');
  }

  toggleBuiltForAgent(agentId: string): void {
    this.builtForAgents.update((current) => {
      if (current.includes(agentId)) {
        const next = current.filter((id) => id !== agentId);
        return next.length > 0 ? next : current;
      }

      return [...current, agentId].sort();
    });
  }

  isBuiltForAgentSelected(agentId: string): boolean {
    return this.builtForAgents().includes(agentId);
  }

  complexityLabel(): string {
    const map: Record<SetupComplexity, string> = {
      beginner: 'Beginner',
      intermediate: 'Intermediate',
      advanced: 'Advanced'
    };
    return map[this.complexity()];
  }

  audienceLabel(): string {
    const map: Record<TargetAudience, string> = {
      frontend: 'Frontend',
      backend: 'Backend',
      fullstack: 'Full Stack',
      devops: 'DevOps',
      'ai-engineering': 'AI Engineering',
      general: 'General'
    };
    return map[this.targetAudience()];
  }

  private applyAnalysisToMetadata(data: AnalysisOutput): void {
    this.aiSuggestedName.set(data.workflowName);
    this.workflowName.set(data.workflowName);
    this.description.set(data.description);
    this.tags.set([...data.tags]);

    if (data.suggestedDependencies.length === 0) {
      this.runsStandalone.set(true);
      this.dependencies.set([]);
    } else {
      this.runsStandalone.set(false);
      this.dependencies.set(
        data.suggestedDependencies.map((name) => ({
          kind: 'mcp' as const,
          name,
          requirement: 'required' as const,
          note: ''
        }))
      );
    }
  }
}
