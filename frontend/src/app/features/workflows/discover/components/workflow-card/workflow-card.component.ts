import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WhAvatarComponent } from '../../../../../shared/ui/avatar/wh-avatar.component';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { WhStarToggleComponent } from '../../../../../shared/ui/star-toggle/wh-star-toggle.component';
import { formatCount } from '../../../../../shared/utils/format-count';
import {
  DISCOVER_COMPONENT_TYPE_LABELS,
  SOURCE_IDE_LABELS
} from '../../constants/discover.constants';
import { WorkflowCard } from '../../models/discover.model';

@Component({
  selector: 'wh-workflow-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent, WhAvatarComponent, WhStarToggleComponent],
  templateUrl: './workflow-card.component.html',
  styleUrl: './workflow-card.component.scss'
})
export class WorkflowCardComponent {
  readonly workflow = input.required<WorkflowCard>();
  readonly starBusy = input(false);
  readonly open = output<string>();
  readonly starToggle = output<string>();

  readonly typeLabels = DISCOVER_COMPONENT_TYPE_LABELS;
  readonly ideLabels = SOURCE_IDE_LABELS;

  componentCountEntries(): [string, number][] {
    return Object.entries(this.workflow().componentCounts).filter(([, count]) => count > 0);
  }

  authorName(): string {
    const author = this.workflow().author;
    return author.displayName?.trim() || author.name;
  }

  builtForAgentsLabel(): string {
    const agents = this.workflow().builtForAgents;
    if (agents.length > 0) {
      return agents.map((agent) => this.ideLabels[agent] ?? agent).join(', ');
    }

    return this.ideLabels[this.workflow().sourceIde] ?? this.workflow().sourceIde;
  }

  typeLabel(type: string): string {
    return this.typeLabels[type as keyof typeof this.typeLabels] ?? type;
  }

  formattedDownloads(): string {
    return formatCount(this.workflow().downloadCount);
  }
}
