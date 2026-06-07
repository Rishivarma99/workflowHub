import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

@Component({
  selector: 'wh-settings-toggle',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      type="button"
      class="set-toggle"
      [class.set-toggle--on]="on()"
      [attr.aria-pressed]="on()"
      (click)="toggled.emit()"
    >
      <span class="set-toggle-knob"></span>
    </button>
  `,
  styles: `
    .set-toggle {
      width: 42px;
      height: 24px;
      border-radius: 999px;
      border: none;
      cursor: pointer;
      flex: 0 0 auto;
      background: var(--grey-300);
      position: relative;
      transition: background 150ms var(--ease-standard);
      padding: 0;
    }

    .set-toggle--on {
      background: var(--brand);
    }

    .set-toggle-knob {
      position: absolute;
      top: 3px;
      left: 3px;
      width: 18px;
      height: 18px;
      border-radius: 50%;
      background: #fff;
      box-shadow: 0 1px 3px rgba(0, 0, 0, 0.25);
      transition: left 150ms var(--ease-standard);
    }

    .set-toggle--on .set-toggle-knob {
      left: 21px;
    }
  `
})
export class SettingsToggleComponent {
  readonly on = input(false);
  readonly toggled = output<void>();
}
