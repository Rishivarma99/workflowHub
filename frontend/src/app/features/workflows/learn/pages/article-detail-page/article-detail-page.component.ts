import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { WhMarkdownComponent } from '../../../../../shared/ui/markdown/wh-markdown.component';
import { ArticleDetail } from '../../api/dtos/article.dto';
import { LearnApiService } from '../../api/learn-api.service';

@Component({
  selector: 'wh-article-detail-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, DatePipe, WhIconComponent, WhMarkdownComponent],
  templateUrl: './article-detail-page.component.html',
  styleUrl: './article-detail-page.component.scss'
})
export class ArticleDetailPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(LearnApiService);
  private readonly notifications = inject(NotificationService);

  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly article = signal<ArticleDetail | null>(null);

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((params) => {
      const slug = params.get('slug')?.trim();
      if (!slug) {
        this.error.set('Article not found.');
        this.loading.set(false);
        return;
      }

      this.loadArticle(slug);
    });
  }

  typeLabel(type: ArticleDetail['contentType']): string {
    return type === 'guide' ? 'Guide' : 'Article';
  }

  authorName(article: ArticleDetail): string {
    return article.author.displayName || article.author.name;
  }

  private loadArticle(slug: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.article.set(null);

    this.api
      .getBySlug(slug)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (detail) => {
          this.article.set(detail);
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
