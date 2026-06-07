import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  HostListener,
  computed,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { NotificationService } from '../../../../../core/services/notification.service';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../../shared/utils/api/to-user-facing-error';
import { SOURCE_IDE_LABELS } from '../../../discover/constants/discover.constants';
import { MyWorkflowRowComponent } from '../../components/my-workflow-row/my-workflow-row.component';
import { MyWorkflowListItem } from '../../models/my-workflows.model';
import { MyWorkflowsApiService } from '../../services/my-workflows-api.service';

@Component({
  selector: 'wh-my-workflows-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent, MyWorkflowRowComponent],
  templateUrl: './my-workflows-page.component.html',
  styleUrl: './my-workflows-page.component.scss'
})
export class MyWorkflowsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly api = inject(MyWorkflowsApiService);
  private readonly notifications = inject(NotificationService);

  readonly ideLabels = SOURCE_IDE_LABELS;

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly items = signal<MyWorkflowListItem[]>([]);
  readonly publishedCount = signal(0);
  readonly totalDownloads = signal(0);
  readonly totalComponents = signal(0);
  readonly openMenuId = signal<string | null>(null);

  readonly hasWorkflows = computed(() => this.items().length > 0);
  readonly formattedTotalDownloads = computed(() => this.totalDownloads().toLocaleString());

  constructor() {
    this.load();
  }

  @HostListener('document:click')
  onDocumentClick(): void {
    this.openMenuId.set(null);
  }

  createWorkflow(): void {
    void this.router.navigate(['/workflows/create']);
  }

  openWorkflow(id: string): void {
    this.openMenuId.set(null);
    void this.router.navigate(['/workflows', id]);
  }

  editWorkflow(id: string): void {
    this.openMenuId.set(null);
    void this.router.navigate(['/workflows', id]);
  }

  onMenuToggle(id: string): void {
    this.openMenuId.update((current) => (current === id ? null : id));
  }

  deleteWorkflow(id: string): void {
    this.openMenuId.set(null);
    const workflow = this.items().find((item) => item.id === id);
    const name = workflow?.name ?? 'this workflow';
    if (!confirm(`Delete "${name}"? This cannot be undone.`)) {
      return;
    }

    this.api
      .delete(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.items.update((list) => list.filter((item) => item.id !== id));
          this.publishedCount.set(this.items().length);
          this.totalDownloads.set(this.items().reduce((sum, item) => sum + item.downloadCount, 0));
          this.totalComponents.set(this.items().reduce((sum, item) => sum + item.componentCount, 0));
          this.notifications.success(`"${name}" deleted.`);
        },
        error: (err) => {
          this.notifications.error(toUserFacingError(err));
        }
      });
  }

  ideLabel(sourceIde: string): string {
    return this.ideLabels[sourceIde] ?? sourceIde;
  }

  updatedLabel(updatedAtUtc: string): string {
    const updated = new Date(updatedAtUtc);
    const diffMs = Date.now() - updated.getTime();
    const minutes = Math.floor(diffMs / 60_000);
    if (minutes < 1) {
      return 'just now';
    }

    if (minutes < 60) {
      return `${minutes}m ago`;
    }

    const hours = Math.floor(minutes / 60);
    if (hours < 24) {
      return `${hours}h ago`;
    }

    const days = Math.floor(hours / 24);
    if (days < 30) {
      return `${days}d ago`;
    }

    return updated.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
  }

  private load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.api
      .getMine()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.items.set(summary.items);
          this.publishedCount.set(summary.publishedCount);
          this.totalDownloads.set(summary.totalDownloads);
          this.totalComponents.set(summary.totalComponentsIndexed);
          this.loading.set(false);
        },
        error: (err) => {
          this.error.set(toUserFacingError(err));
          this.loading.set(false);
          this.notifications.error(toUserFacingError(err));
        }
      });
  }
}
