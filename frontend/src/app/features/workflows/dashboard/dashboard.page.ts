import { Component } from '@angular/core';

/**
 * Placeholder dashboard page for the Workflow Hub business area.
 * No business logic yet - exists so the lazy route resolves cleanly.
 */
@Component({
  selector: 'wh-dashboard-page',
  template: `
    <section class="flex flex-col gap-4 p-4 sm:p-6">
      <h1 class="text-lg font-semibold sm:text-xl">Dashboard</h1>
    </section>
  `
})
export class DashboardPage {}
