import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { WorkflowCardComponent } from '../../components/workflow-card/workflow-card.component';
import {
  DISCOVER_COMPONENT_TYPE_LABELS,
  DISCOVER_COMPONENT_TYPES,
  DISCOVER_PAGE_SIZE
} from '../../constants/discover.constants';
import { WorkflowBrowseCriteria } from '../../models/discover.model';
import { DiscoverApiService } from '../../services/discover-api.service';

@Component({
  selector: 'wh-browse-all-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent, WorkflowCardComponent],
  templateUrl: './browse-all-page.component.html',
  styleUrl: './browse-all-page.component.scss'
})
export class BrowseAllPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(DiscoverApiService);
  private readonly notifications = inject(NotificationService);

  readonly typeLabels = DISCOVER_COMPONENT_TYPE_LABELS;
  readonly componentTypes = DISCOVER_COMPONENT_TYPES;
  readonly pageSize = DISCOVER_PAGE_SIZE;

  readonly selectedTypes = signal<string[]>([]);
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly workflows = signal<import('../../models/discover.model').WorkflowCard[]>([]);
  readonly total = signal(0);

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.total() / this.pageSize)));
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
        this.page.set(criteria.page);
        this.selectedTypes.set(criteria.types ?? []);
        this.loadPage(criteria);
      });
  }

  toggleType(type: string): void {
    const current = this.selectedTypes();
    const next = current.includes(type) ? current.filter((t) => t !== type) : [...current, type];
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        types: next.length > 0 ? next.join(',') : null,
        page: 1
      },
      queryParamsHandling: 'merge'
    });
  }

  isTypeSelected(type: string): boolean {
    return this.selectedTypes().includes(type);
  }

  typeLabel(type: string): string {
    return this.typeLabels[type as keyof typeof this.typeLabels] ?? type;
  }

  goBack(): void {
    void this.router.navigate(['/workflows/discover']);
  }

  openWorkflow(id: string): void {
    void this.router.navigate(['/workflows', id]);
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

  private criteriaFromUrl(params: ParamMap): WorkflowBrowseCriteria {
    const types = params.get('types')?.split(',').filter(Boolean) ?? [];
    return {
      page: Math.max(1, Number(params.get('page') ?? 1)),
      pageSize: this.pageSize,
      types: types.length > 0 ? types : undefined
    };
  }

  private loadPage(criteria: WorkflowBrowseCriteria): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .browse(criteria)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.workflows.set(result.items);
          this.total.set(result.total);
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
