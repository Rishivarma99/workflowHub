import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

@Component({
  selector: 'wh-avatar',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span
      class="inline-flex items-center justify-center rounded-full font-bold text-white"
      [style.width.px]="size()"
      [style.height.px]="size()"
      [style.fontSize.px]="fontSize()"
      [style.background]="color()"
      [attr.aria-label]="name()"
    >
      {{ initials() }}
    </span>
  `
})
export class WhAvatarComponent {
  readonly name = input.required<string>();
  readonly size = input(32);
  readonly color = input('#5353ef');

  readonly fontSize = computed(() => Math.round(this.size() * 0.38));

  readonly initials = computed(() => {
    const parts = this.name().trim().split(/\s+/).filter(Boolean);
    if (parts.length === 0) return '?';
    if (parts.length === 1) return parts[0].slice(0, 2).toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });
}
