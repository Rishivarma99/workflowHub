import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ShellUser, WorkflowsSidebarComponent } from './sidebar/workflows-sidebar.component';
import { WorkflowsTopbarComponent } from './topbar/workflows-topbar.component';
import { WorkflowsTabbarComponent } from './footer/workflows-tabbar.component';

/**
 * Authenticated Workflow Hub shell: sidebar (desktop), top bar, main outlet, tab bar (mobile).
 */
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
  private readonly router = inject(Router);

  /** Placeholder until AuthService provides the signed-in user. */
  readonly user = signal<ShellUser>({
    name: 'Jamie Lo',
    email: 'jamie@acme.co',
    avatarColor: '#5353ef'
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
    // TODO(auth): clear tokens + call POST /auth/logout
    void this.router.navigate(['/auth/login']);
  }
}
