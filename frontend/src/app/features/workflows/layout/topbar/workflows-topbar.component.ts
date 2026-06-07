import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { WhAvatarComponent } from '../../../../shared/ui/avatar/wh-avatar.component';
import { WhIconComponent } from '../../../../shared/ui/icon/wh-icon.component';
import { ShellUser } from '../sidebar/workflows-sidebar.component';
import { WORKFLOWS_REGISTRY_NAV } from '../sidebar/workflows-nav.config';

@Component({
  selector: 'wh-workflows-topbar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, WhIconComponent, WhAvatarComponent],
  templateUrl: './workflows-topbar.component.html',
  styleUrl: './workflows-topbar.component.scss'
})
export class WorkflowsTopbarComponent {
  readonly user = input.required<ShellUser>();
  readonly openSettings = output<void>();
  readonly signOut = output<void>();

  readonly nav = WORKFLOWS_REGISTRY_NAV;
}
