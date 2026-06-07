import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import { WhStarToggleComponent } from '../../../../../shared/ui/star-toggle/wh-star-toggle.component';
import { AGENT_ASSET_TYPE_LABELS } from '../../constants/agent-assets.constants';
import { AgentAssetCard } from '../../models/agent-asset.model';

@Component({
  selector: 'wh-agent-asset-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent, WhStarToggleComponent],
  templateUrl: './agent-asset-card.component.html',
  styleUrl: './agent-asset-card.component.scss'
})
export class AgentAssetCardComponent {
  readonly asset = input.required<AgentAssetCard>();
  readonly starBusy = input(false);
  readonly open = output<{ workflowId: string; componentId: string }>();
  readonly starToggle = output<string>();

  readonly typeLabels = AGENT_ASSET_TYPE_LABELS;

  typeLabel(type: string): string {
    return this.typeLabels[type as keyof typeof this.typeLabels] ?? type;
  }
}
