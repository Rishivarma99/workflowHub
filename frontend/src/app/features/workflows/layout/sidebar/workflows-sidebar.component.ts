import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { WhAvatarComponent } from '../../../../shared/ui/avatar/wh-avatar.component';
import { WhIconComponent } from '../../../../shared/ui/icon/wh-icon.component';
import { WORKFLOWS_REGISTRY_NAV } from './workflows-nav.config';

export interface ShellUser {
  name: string;
  email: string;
  avatarColor: string;
}

@Component({
  selector: 'wh-workflows-sidebar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, WhIconComponent, WhAvatarComponent],
  templateUrl: './workflows-sidebar.component.html',
  styleUrl: './workflows-sidebar.component.scss'
})
export class WorkflowsSidebarComponent {
  readonly user = input.required<ShellUser>();
  readonly createWorkflow = output<void>();
  readonly openSettings = output<void>();

  readonly nav = WORKFLOWS_REGISTRY_NAV;
}
