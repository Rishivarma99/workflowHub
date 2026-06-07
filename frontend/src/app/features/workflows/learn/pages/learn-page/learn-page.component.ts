import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, ParamMap, Router, RouterLink } from '@angular/router';
import { distinctUntilChanged, map } from 'rxjs';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { ArticleCardComponent } from '../../components/article-card/article-card.component';
import { LEARN_PAGE_SIZE } from '../../constants/learn.constants';
import { ArticleCard, ArticleCategory } from '../../api/dtos/article.dto';
import { ArticleSearchCriteria } from '../../api/requests/article-search-criteria';
import { LearnApiService } from '../../api/learn-api.service';

@Component({
  selector: 'wh-learn-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, WhIconComponent, ArticleCardComponent],
  templateUrl: './learn-page.component.html',
  styleUrl: './learn-page.component.scss'
})
export class LearnPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly api = inject(LearnApiService);
  private readonly notifications = inject(NotificationService);

  readonly pageSize = LEARN_PAGE_SIZE;

  readonly query = signal('');
  readonly searchDraft = signal('');
  readonly category = signal<string | null>(null);
  readonly page = signal(1);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly pinnedGuides = signal<ArticleCard[]>([]);
  readonly latestArticles = signal<ArticleCard[]>([]);
  readonly categories = signal<ArticleCategory[]>([]);
  readonly searchResults = signal<ArticleCard[]>([]);
  readonly searchTotal = signal(0);

  readonly searching = computed(() => this.query().trim().length > 0);
  readonly filtering = computed(() => !!this.category());
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.searchTotal() / this.pageSize)));
  readonly hasPrev = computed(() => this.page() > 1);
  readonly hasNext = computed(() => this.page() < this.totalPages());

  constructor() {
    this.loadCategories();

    this.route.queryParamMap
      .pipe(
        map((params) => this.criteriaFromUrl(params)),
        distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((criteria) => {
        this.query.set(criteria.query ?? '');
        this.searchDraft.set(criteria.query ?? '');
        this.category.set(
          typeof criteria.filters['category'] === 'string' ? criteria.filters['category'] : null
        );
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
      queryParams: { q: q || null, page: q || this.category() ? 1 : null },
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

  selectCategory(slug: string | null): void {
    const next = this.category() === slug ? null : slug;
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: next, page: next ? 1 : null },
      queryParamsHandling: 'merge'
    });
  }

  openArticle(slug: string): void {
    void this.router.navigate(['/workflows/learn', slug], {
      queryParams: this.buildReturnQuery()
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

  isCategoryActive(slug: string): boolean {
    return this.category() === slug;
  }

  private criteriaFromUrl(params: ParamMap): ArticleSearchCriteria {
    const q = params.get('q')?.trim();
    const category = params.get('category')?.trim();
    const filters: Record<string, string | number | boolean | undefined> = {};
    if (category) {
      filters['category'] = category;
    }

    return {
      query: q || undefined,
      page: Math.max(1, Number(params.get('page') ?? 1)),
      pageSize: this.pageSize,
      filters
    };
  }

  private loadForCriteria(criteria: ArticleSearchCriteria): void {
    const categoryFilter = criteria.filters['category'];
    if (criteria.query || categoryFilter) {
      this.runSearch(criteria);
      return;
    }

    this.searchResults.set([]);
    this.searchTotal.set(0);
    this.loadHome();
  }

  private loadCategories(): void {
    this.api
      .getHome()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (home) => this.categories.set(home.categories),
        error: () => undefined
      });
  }

  private loadHome(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getHome()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (home) => {
          this.pinnedGuides.set(home.pinnedGuides);
          this.latestArticles.set(home.latestArticles);
          this.categories.set(home.categories);
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

  private runSearch(criteria: ArticleSearchCriteria): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .search(criteria)
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

  private buildReturnQuery(): Record<string, string | number | null> {
    const q = this.query().trim();
    const category = this.category();
    return {
      q: q || null,
      category: category || null,
      page: this.page() > 1 ? this.page() : null
    };
  }
}
