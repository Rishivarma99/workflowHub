import { Component } from '@angular/core';

/**
 * Placeholder landing page. Renders an empty shell until real designs land.
 * Intentionally contains no business logic.
 */
@Component({
  selector: 'wh-home-page',
  template: `
    <main class="flex min-h-screen flex-col items-center justify-center gap-2 p-6">
      <h1 class="text-xl font-semibold sm:text-2xl">Workflow Hub</h1>
      <p class="text-sm text-[color:var(--color-text-muted)]">Scaffold ready.</p>
    </main>
  `
})
export class HomePage {}
