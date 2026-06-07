import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WhIconComponent } from '../icon/wh-icon.component';
import { formatCount } from '../../utils/format-count';

@Component({
  selector: 'wh-star-toggle',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  template: `
    <button
      type="button"
      class="star-btn"
      [class.on]="starred()"
      [disabled]="disabled()"
      [attr.aria-pressed]="starred()"
      aria-label="Toggle star"
      (click)="onClick($event)"
    >
      <wh-icon name="star" [size]="16" />
      <span>{{ formattedCount() }}</span>
    </button>
  `,
  styles: [
    `
      .star-btn {
        display: inline-flex;
        align-items: center;
        gap: 0.3125rem;
        padding: 0.125rem 0.375rem;
        border: none;
        border-radius: 999px;
        background: transparent;
        color: var(--fg-2);
        font-size: 0.75rem;
        font-weight: 600;
        cursor: pointer;
      }

      .star-btn:disabled {
        opacity: 0.55;
        cursor: not-allowed;
      }

      .star-btn.on {
        color: var(--amber-600, #d97706);
      }

      .star-btn.on wh-icon {
        fill: currentColor;
      }
    `
  ]
})
export class WhStarToggleComponent {
  readonly starred = input(false);
  readonly count = input(0);
  readonly disabled = input(false);
  readonly toggle = output<void>();

  formattedCount(): string {
    return formatCount(this.count());
  }

  onClick(event: Event): void {
    event.stopPropagation();
    this.toggle.emit();
  }
}
