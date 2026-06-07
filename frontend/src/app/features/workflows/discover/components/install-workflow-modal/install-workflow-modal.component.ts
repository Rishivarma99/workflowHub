import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import {
  DISCOVER_COMPONENT_TYPE_LABELS,
  INSTALL_LEVEL_OPTIONS,
  INSTALL_TARGET_AGENT_OPTIONS,
  SOURCE_IDE_LABELS
} from '../../constants/discover.constants';
import { InstallLevel, InstallTargetAgent, WorkflowDetail } from '../../models/discover.model';

export interface InstallModalConfirm {
  targetAgent: InstallTargetAgent;
  installLevel: InstallLevel;
}

@Component({
  selector: 'wh-install-workflow-modal',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  templateUrl: './install-workflow-modal.component.html',
  styleUrl: './install-workflow-modal.component.scss'
})
export class InstallWorkflowModalComponent {
  readonly workflow = input.required<WorkflowDetail>();
  readonly generating = input(false);
  readonly generatedPrompt = input<string | null>(null);

  readonly closed = output<void>();
  readonly confirm = output<InstallModalConfirm>();

  readonly targetAgentOptions = INSTALL_TARGET_AGENT_OPTIONS;
  readonly installLevelOptions = INSTALL_LEVEL_OPTIONS;
  readonly typeLabels = DISCOVER_COMPONENT_TYPE_LABELS;
  readonly ideLabels = SOURCE_IDE_LABELS;

  readonly targetAgent = signal<InstallTargetAgent>('same');
  readonly installLevel = signal<InstallLevel>('project');

  readonly componentCounts = computed(() => {
    const counts = new Map<string, number>();
    for (const component of this.workflow().components) {
      counts.set(component.componentType, (counts.get(component.componentType) ?? 0) + 1);
    }
    return counts;
  });

  readonly mcpCount = computed(
    () => this.workflow().dependencies.filter((dep) => dep.kind === 'mcp').length
  );

  readonly pluginCount = computed(
    () => this.workflow().dependencies.filter((dep) => dep.kind === 'plugin').length
  );

  readonly builtForLabel = computed(() =>
    this.workflow()
      .builtForAgents.map((agent) => this.ideLabels[agent] ?? agent)
      .join(', ')
  );

  assetCountEntries(): [string, number][] {
    return [...this.componentCounts().entries()].filter(([, count]) => count > 0);
  }

  typeLabel(type: string): string {
    return this.typeLabels[type as keyof typeof this.typeLabels] ?? type;
  }

  onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget) {
      this.closed.emit();
    }
  }

  generatePrompt(): void {
    this.confirm.emit({
      targetAgent: this.targetAgent(),
      installLevel: this.installLevel()
    });
  }
}
