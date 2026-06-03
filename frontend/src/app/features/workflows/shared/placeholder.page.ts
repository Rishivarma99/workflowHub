import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { map } from 'rxjs';

/** Temporary route target until feature screens are implemented. */
@Component({
  selector: 'wh-placeholder-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="content-inner">
      <header class="page-head">
        <h1>{{ page().title }}</h1>
        @if (page().subtitle) {
          <p class="sub">{{ page().subtitle }}</p>
        }
      </header>
      <p class="text-[var(--fg-2)] text-sm">Screen content coming soon.</p>
    </div>
  `,
  styleUrl: './placeholder.page.scss'
})
export class PlaceholderPageComponent {
  private readonly route = inject(ActivatedRoute);

  readonly page = toSignal(
    this.route.data.pipe(
      map((data) => ({
        title: (data['title'] as string) ?? '',
        subtitle: (data['subtitle'] as string) ?? ''
      }))
    ),
    { initialValue: { title: '', subtitle: '' } }
  );
}
