import { ChangeDetectionStrategy, Component, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { WhIconComponent } from '../../../../shared/ui/icon/wh-icon.component';
import { WORKFLOWS_TAB_NAV } from '../sidebar/workflows-nav.config';

@Component({
  selector: 'wh-workflows-tabbar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterLink, RouterLinkActive, WhIconComponent],
  templateUrl: './workflows-tabbar.component.html',
  styleUrl: './workflows-tabbar.component.scss'
})
export class WorkflowsTabbarComponent {
  readonly createWorkflow = output<void>();
  readonly tabs = WORKFLOWS_TAB_NAV.filter((t) => t.tabBar);
}
