import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { ShellUser, WorkflowsSidebarComponent } from './sidebar/workflows-sidebar.component';
import { WorkflowsTopbarComponent } from './topbar/workflows-topbar.component';
import { WorkflowsTabbarComponent } from './footer/workflows-tabbar.component';

@Component({
  selector: 'wh-workflows-layout',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterOutlet,
    WorkflowsSidebarComponent,
    WorkflowsTopbarComponent,
    WorkflowsTabbarComponent
  ],
  templateUrl: './workflows-layout.component.html',
  styleUrl: './workflows-layout.component.scss'
})
export class WorkflowsLayoutComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);

  readonly user = computed<ShellUser>(() => {
    const user = this.auth.currentUser();
    return {
      name: user?.displayName || user?.name || 'Workflow Hub User',
      email: user?.email || '',
      avatarColor: '#5353ef',
      avatarUrl: user?.avatarUrl ?? null
    };
  });

  readonly searchQuery = signal('');

  onCreateWorkflow(): void {
    void this.router.navigate(['/workflows/create']);
  }

  onOpenSettings(): void {
    void this.router.navigate(['/workflows/settings']);
  }

  onSubmitSearch(): void {
    const q = this.searchQuery().trim();
    void this.router.navigate(['/workflows/search'], { queryParams: q ? { q } : {} });
  }

  onSignOut(): void {
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => this.auth.hardLogout()
      });
  }
}
