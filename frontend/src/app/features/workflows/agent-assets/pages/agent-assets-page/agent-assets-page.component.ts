import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { AgentAssetCardComponent } from '../../components/agent-asset-card/agent-asset-card.component';
import {
  AGENT_ASSET_FILTER_TYPES,
  AGENT_ASSET_TYPE_LABELS,
  AGENT_ASSETS_PAGE_SIZE
} from '../../constants/agent-assets.constants';
import { AgentAssetCard, AgentAssetSearchCriteria } from '../../models/agent-asset.model';
import { AgentAssetsApiService } from '../../services/agent-assets-api.service';
import { AgentAssetEngagementApiService } from '../../services/agent-asset-engagement-api.service';

@Component({
  selector: 'wh-agent-assets-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WhIconComponent, AgentAssetCardComponent],
  templateUrl: './agent-assets-page.component.html',
  styleUrl: './agent-assets-page.component.scss'
})
export class AgentAssetsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(AgentAssetsApiService);
  private readonly engagement = inject(AgentAssetEngagementApiService);
  private readonly notifications = inject(NotificationService);

  readonly typeLabels = AGENT_ASSET_TYPE_LABELS;
  readonly filterTypes = AGENT_ASSET_FILTER_TYPES;
  readonly pageSize = AGENT_ASSETS_PAGE_SIZE;

  readonly query = signal('');
  readonly searchDraft = signal('');
  readonly page = signal(1);
  readonly selectedTypes = signal<string[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly popular = signal<AgentAssetCard[]>([]);
  readonly recent = signal<AgentAssetCard[]>([]);
  readonly searchResults = signal<AgentAssetCard[]>([]);
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
        this.selectedTypes.set(this.typesFromFilters(criteria.filters));
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

  openAsset(event: { workflowId: string; componentId: string }): void {
    void this.router.navigate(['/workflows', event.workflowId], {
      queryParams: {
        componentId: event.componentId,
        ...(this.searching() ? { q: this.query().trim() } : {})
      }
    });
  }

  toggleStar(assetId: string): void {
    const asset = this.findAsset(assetId);
    if (!asset || this.starBusyId()) {
      return;
    }

    this.starBusyId.set(assetId);
    const request = asset.isStarred
      ? this.engagement.unstar(assetId)
      : this.engagement.star(assetId);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (result) => {
        this.patchAsset(assetId, {
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

  private criteriaFromUrl(params: ParamMap): AgentAssetSearchCriteria {
    const q = params.get('q')?.trim();
    const types = params.get('types')?.split(',').filter(Boolean) ?? [];
    return {
      query: q || undefined,
      page: Math.max(1, Number(params.get('page') ?? 1)),
      pageSize: this.pageSize,
      sortBy: q ? 'relevance' : 'stars',
      filters: types.length > 0 ? { assetTypes: types.join(',') } : {}
    };
  }

  private typesFromFilters(filters: AgentAssetSearchCriteria['filters']): string[] {
    const raw = filters['assetTypes'];
    if (typeof raw !== 'string' || !raw.trim()) {
      return [];
    }

    return raw.split(',').filter(Boolean);
  }

  private loadForCriteria(criteria: AgentAssetSearchCriteria): void {
    if (criteria.query) {
      this.runSearch(criteria);
      return;
    }

    const types = this.typesFromFilters(criteria.filters);
    if (types.length > 0) {
      this.runBrowse(criteria.page, types);
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
          this.popular.set(home.popular);
          this.recent.set(home.recent);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(toUserFacingError(err));
          this.loading.set(false);
        }
      });
  }

  private runBrowse(page: number, types: string[]): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .browse({
        page,
        pageSize: this.pageSize,
        types
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

  private runSearch(criteria: AgentAssetSearchCriteria): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .search({
        ...criteria,
        sortBy: criteria.query ? 'relevance' : 'stars'
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

  private findAsset(assetId: string): AgentAssetCard | undefined {
    return (
      this.searchResults().find((item) => item.id === assetId) ??
      this.popular().find((item) => item.id === assetId) ??
      this.recent().find((item) => item.id === assetId)
    );
  }

  private patchAsset(assetId: string, patch: Partial<AgentAssetCard>): void {
    const apply = (items: AgentAssetCard[]) =>
      items.map((item) => (item.id === assetId ? { ...item, ...patch } : item));

    this.searchResults.update(apply);
    this.popular.update(apply);
    this.recent.update(apply);
  }
}
