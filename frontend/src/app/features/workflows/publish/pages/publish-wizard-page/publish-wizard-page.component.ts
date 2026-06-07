import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { SlicePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WhIconComponent } from '../../../../../shared/ui/icon/wh-icon.component';
import {
  AUDIENCE_OPTIONS,
  COMPLEXITY_OPTIONS,
  COMPONENT_TYPE_LABELS,
  PUBLISH_WIZARD_STEPS,
  SOURCE_IDE_OPTIONS
} from '../../constants/publish.constants';
import { PublishWizardStore } from '../../services/publish-wizard.store';

@Component({
  selector: 'wh-publish-wizard-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [PublishWizardStore],
  imports: [FormsModule, SlicePipe, WhIconComponent],
  templateUrl: './publish-wizard-page.component.html',
  styleUrl: './publish-wizard-page.component.scss'
})
export class PublishWizardPageComponent {
  readonly store = inject(PublishWizardStore);

  readonly steps = PUBLISH_WIZARD_STEPS;
  readonly sourceIdes = SOURCE_IDE_OPTIONS;
  readonly complexityOptions = COMPLEXITY_OPTIONS;
  readonly audienceOptions = AUDIENCE_OPTIONS;
  readonly componentTypeLabels = COMPONENT_TYPE_LABELS;

  onTagKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      event.preventDefault();
      this.store.addTag();
    }
  }

  validateAndContinueFromAnalysis(): void {
    if (this.store.validateAnalysisPaste()) {
      this.store.setStep(2);
    }
  }

  componentCountEntries(): [string, number][] {
    return [...this.store.componentCounts().entries()];
  }

  componentTypeLabel(type: string): string {
    return this.componentTypeLabels[type as keyof typeof this.componentTypeLabels] ?? type;
  }
}
