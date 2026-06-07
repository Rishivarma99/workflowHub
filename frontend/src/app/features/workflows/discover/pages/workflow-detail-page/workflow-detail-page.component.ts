import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { map } from 'rxjs';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhAvatarComponent } from '../../../../../shared/ui/avatar/wh-avatar.component';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { WhStarToggleComponent } from '../../../../../shared/ui/star-toggle/wh-star-toggle.component';
import { formatCount } from '../../../../../shared/utils/format-count';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { WorkflowEngagementApiService } from '../../../shared/services/workflow-engagement-api.service';
import { AgentAssetEngagementApiService } from '../../../agent-assets/services/agent-asset-engagement-api.service';
import {
  DISCOVER_COMPONENT_TYPE_LABELS,
  SOURCE_IDE_LABELS
} from '../../constants/discover.constants';
import { WorkflowComponentDetail, WorkflowDetail } from '../../models/discover.model';
import { DiscoverApiService } from '../../services/discover-api.service';
import { InstallWorkflowModalComponent } from '../../components/install-workflow-modal/install-workflow-modal.component';
import { InstallLevel, InstallTargetAgent } from '../../models/discover.model';

@Component({
  selector: 'wh-workflow-detail-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DatePipe, WhIconComponent, WhAvatarComponent, WhStarToggleComponent, InstallWorkflowModalComponent],
  templateUrl: './workflow-detail-page.component.html',
  styleUrl: './workflow-detail-page.component.scss'
})
export class WorkflowDetailPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(DiscoverApiService);
  private readonly engagement = inject(WorkflowEngagementApiService);
  private readonly assetEngagement = inject(AgentAssetEngagementApiService);
  private readonly notifications = inject(NotificationService);

  readonly typeLabels = DISCOVER_COMPONENT_TYPE_LABELS;
  readonly ideLabels = SOURCE_IDE_LABELS;

  readonly workflowId = toSignal(
    this.route.paramMap.pipe(map((params) => params.get('workflowId') ?? '')),
    { initialValue: '' }
  );

  readonly highlightQuery = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('q') ?? '')),
    { initialValue: '' }
  );

  readonly focusComponentId = toSignal(
    this.route.queryParamMap.pipe(map((params) => params.get('componentId') ?? '')),
    { initialValue: '' }
  );

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly workflow = signal<WorkflowDetail | null>(null);
  readonly expandedComponentId = signal<string | null>(null);
  readonly installModalOpen = signal(false);
  readonly installGenerating = signal(false);
  readonly installPrompt = signal<string | null>(null);
  readonly installCopied = signal(false);
  readonly starBusy = signal(false);
  readonly componentStarBusyId = signal<string | null>(null);

  readonly builtForLabel = computed(() => {
    const wf = this.workflow();
    if (!wf) {
      return '';
    }

    return wf.builtForAgents.map((agent) => this.ideLabel(agent)).join(', ');
  });

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.loadDetail();
    });
  }

  authorName(wf: WorkflowDetail): string {
    return wf.author.displayName?.trim() || wf.author.name;
  }

  ideLabel(ide: string): string {
    return this.ideLabels[ide] ?? ide;
  }

  typeLabel(type: string): string {
    return this.typeLabels[type as keyof typeof this.typeLabels] ?? type;
  }

  repoDisplay(url: string): string {
    return url.replace(/^https?:\/\//, '');
  }

  repoName(url: string): string {
    const parts = url.replace(/\/$/, '').split('/');
    return parts[parts.length - 1] ?? 'repo';
  }

  formattedCount(value: number): string {
    return formatCount(value);
  }

  goBack(): void {
    void this.router.navigate(['/workflows/discover'], {
      queryParams: this.highlightQuery() ? { q: this.highlightQuery() } : {}
    });
  }

  toggleComponent(component: WorkflowComponentDetail): void {
    this.expandedComponentId.update((current) =>
      current === component.id ? null : component.id
    );
  }

  isExpanded(component: WorkflowComponentDetail): boolean {
    return this.expandedComponentId() === component.id;
  }

  onInstallClick(): void {
    this.installPrompt.set(null);
    this.installModalOpen.set(true);
  }

  closeInstallModal(): void {
    this.installModalOpen.set(false);
    this.installGenerating.set(false);
  }

  onInstallConfirm(options: { targetAgent: InstallTargetAgent; installLevel: InstallLevel }): void {
    const id = this.workflowId();
    if (!id || this.installGenerating()) {
      return;
    }

    this.installGenerating.set(true);

    this.api
      .generateInstallPrompt(id, options)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: async (result) => {
          this.installPrompt.set(result.prompt);
          this.installGenerating.set(false);

          try {
            await navigator.clipboard.writeText(result.prompt);
            this.installCopied.set(true);
            this.notifications.success('Installation prompt copied — paste it into your agent');
            setTimeout(() => this.installCopied.set(false), 2000);
          } catch {
            this.notifications.error('Prompt generated but could not copy to clipboard');
          }

          this.engagement
            .recordDownload(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: (downloadResult) => {
                this.workflow.update((current) =>
                  current ? { ...current, downloadCount: downloadResult.downloadCount } : current
                );
              },
              error: (err) => {
                this.notifications.error(toUserFacingError(err));
              }
            });
        },
        error: (err) => {
          this.installGenerating.set(false);
          this.notifications.error(toUserFacingError(err));
        }
      });
  }

  toggleStar(): void {
    const wf = this.workflow();
    const id = this.workflowId();
    if (!wf || !id || this.starBusy()) {
      return;
    }

    this.starBusy.set(true);
    const request = wf.isStarred ? this.engagement.unstar(id) : this.engagement.star(id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.workflow.update((current) =>
          current
            ? { ...current, isStarred: result.isStarred, starCount: result.starCount }
            : current
        );
        this.starBusy.set(false);
      },
      error: (err) => {
        this.starBusy.set(false);
        this.notifications.error(toUserFacingError(err));
      }
    });
  }

  toggleComponentStar(componentId: string): void {
    const wf = this.workflow();
    if (!wf || this.componentStarBusyId()) {
      return;
    }

    const component = wf.components.find((item) => item.id === componentId);
    if (!component) {
      return;
    }

    this.componentStarBusyId.set(componentId);
    const request = component.isStarred
      ? this.assetEngagement.unstar(componentId)
      : this.assetEngagement.star(componentId);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.workflow.update((current) =>
          current
            ? {
                ...current,
                components: current.components.map((item) =>
                  item.id === componentId
                    ? { ...item, isStarred: result.isStarred, starCount: result.starCount }
                    : item
                )
              }
            : current
        );
        this.componentStarBusyId.set(null);
      },
      error: (err) => {
        this.componentStarBusyId.set(null);
        this.notifications.error(toUserFacingError(err));
      }
    });
  }

  private loadDetail(): void {
    const id = this.workflowId();
    if (!id) {
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    this.api
      .getDetail(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.workflow.set(detail);
          const focusId = this.focusComponentId();
          const initial =
            focusId && detail.components.some((c) => c.id === focusId)
              ? focusId
              : detail.components[0]?.id ?? null;
          this.expandedComponentId.set(initial);
          this.loading.set(false);
        },
        error: (err) => {
          const message = toUserFacingError(err);
          this.error.set(message);
          this.loading.set(false);
          this.notifications.error(message);
        }
      });
  }
}
