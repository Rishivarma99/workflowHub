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
  readonly searchQuery = input('');
  readonly searchQueryChange = output<string>();
  readonly submitSearch = output<void>();
  readonly openSettings = output<void>();
  readonly signOut = output<void>();

  readonly nav = WORKFLOWS_REGISTRY_NAV;

  onSearchInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.searchQueryChange.emit(value);
  }

  onSearchSubmit(event: Event): void {
    event.preventDefault();
    this.submitSearch.emit();
  }
}
