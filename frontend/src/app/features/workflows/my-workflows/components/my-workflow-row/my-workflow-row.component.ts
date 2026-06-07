import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { MyWorkflowListItem } from '../../models/my-workflows.model';

@Component({
  selector: 'wh-my-workflow-row',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  templateUrl: './my-workflow-row.component.html',
  styleUrl: './my-workflow-row.component.scss'
})
export class MyWorkflowRowComponent {
  readonly workflow = input.required<MyWorkflowListItem>();
  readonly ideLabel = input.required<string>();
  readonly updatedLabel = input.required<string>();
  readonly menuOpen = input(false);

  readonly open = output<string>();
  readonly edit = output<string>();
  readonly delete = output<string>();
  readonly menuToggle = output<string>();
}
