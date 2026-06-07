import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { WorkflowCardComponent } from '../../components/workflow-card/workflow-card.component';
import {
  DISCOVER_PAGE_SIZE
} from '../../constants/discover.constants';
import { WorkflowCard, WorkflowSearchCriteria } from '../../models/discover.model';
import { DiscoverApiService } from '../../services/discover-api.service';
import { WorkflowEngagementApiService } from '../../../shared/services/workflow-engagement-api.service';

@Component({
  selector: 'wh-discover-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WhIconComponent, WorkflowCardComponent],
  templateUrl: './discover-page.component.html',
  styleUrl: './discover-page.component.scss'
})
export class DiscoverPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(DiscoverApiService);
  private readonly engagement = inject(WorkflowEngagementApiService);
  private readonly notifications = inject(NotificationService);

  readonly pageSize = DISCOVER_PAGE_SIZE;

  readonly query = signal('');
  readonly searchDraft = signal('');
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly trending = signal<WorkflowCard[]>([]);
  readonly recent = signal<WorkflowCard[]>([]);
  readonly searchResults = signal<WorkflowCard[]>([]);
  readonly searchTotal = signal(0);
  readonly starBusyId = signal<string | null>(null);

  readonly searching = computed(() => this.query().trim().length > 0);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.searchTotal() / this.pageSize)));
  readonly hasPrev = computed(() => this.page() > 1);
  readonly hasNext = computed(() => this.page() < this.totalPages());

  constructor() {
    this.route.queryParamMap
      .pipe(
        map((params) => this.criteriaFromUrl(params)),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((criteria) => {
        this.query.set(criteria.query ?? '');
        this.searchDraft.set(criteria.query ?? '');
        this.page.set(criteria.page);
        this.loadForCriteria(criteria);
      });
  }

  onDraftInput(event: Event): void {
    this.searchDraft.set((event.target as HTMLInputElement).value);
  }

  submitSearch(event: Event): void {
    event.preventDefault();
    const q = this.searchDraft().trim();
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: q || null, page: q ? 1 : null },
      queryParamsHandling: 'merge'
    });
  }

  clearSearch(): void {
    this.searchDraft.set('');
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { q: null, page: null },
      queryParamsHandling: 'merge'
    });
  }

  openWorkflow(id: string): void {
    void this.router.navigate(['/workflows', id], {
      queryParams: this.searching() ? { q: this.query().trim() } : {}
    });
  }

  toggleStar(workflowId: string): void {
    const workflow = this.findWorkflow(workflowId);
    if (!workflow || this.starBusyId()) {
      return;
    }

    this.starBusyId.set(workflowId);
    const request = workflow.isStarred
      ? this.engagement.unstar(workflowId)
      : this.engagement.star(workflowId);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.patchWorkflow(workflowId, {
          isStarred: result.isStarred,
          starCount: result.starCount
        });
        this.starBusyId.set(null);
      },
      error: (err) => {
        this.starBusyId.set(null);
        this.notifications.error(toUserFacingError(err));
      }
    });
  }

  prevPage(): void {
    if (this.hasPrev()) {
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { page: this.page() - 1 },
        queryParamsHandling: 'merge'
      });
    }
  }

  nextPage(): void {
    if (this.hasNext()) {
      void this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { page: this.page() + 1 },
        queryParamsHandling: 'merge'
      });
    }
  }

  private criteriaFromUrl(params: ParamMap): WorkflowSearchCriteria {
    const q = params.get('q')?.trim();
    return {
      query: q || undefined,
      page: Math.max(1, Number(params.get('page') ?? 1)),
      pageSize: this.pageSize,
      sortBy: q ? 'relevance' : 'downloads',
      filters: {}
    };
  }

  private loadForCriteria(criteria: WorkflowSearchCriteria): void {
    if (criteria.query) {
      this.runSearch(criteria);
      return;
    }

    this.searchResults.set([]);
    this.searchTotal.set(0);
    this.loadHomeSections();
  }

  private loadHomeSections(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getHome()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (home) => {
          this.trending.set(home.trending);
          this.recent.set(home.recent);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(toUserFacingError(err));
          this.loading.set(false);
        }
      });
  }

  private runSearch(criteria: WorkflowSearchCriteria): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .search({
        ...criteria,
        sortBy: 'relevance'
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.searchResults.set(result.items);
          this.searchTotal.set(result.total);
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

  private findWorkflow(workflowId: string): WorkflowCard | undefined {
    return (
      this.searchResults().find((item) => item.id === workflowId) ??
      this.trending().find((item) => item.id === workflowId) ??
      this.recent().find((item) => item.id === workflowId)
    );
  }

  private patchWorkflow(workflowId: string, patch: Partial<WorkflowCard>): void {
    const apply = (items: WorkflowCard[]) =>
      items.map((item) => (item.id === workflowId ? { ...item, ...patch } : item));

    this.searchResults.update(apply);
    this.trending.update(apply);
    this.recent.update(apply);
  }
}
