import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * Authenticated business shell for the Workflow Hub area.
 * Owns the responsive page chrome (nav/header) once designs land.
 * For now it only hosts the child router outlet.
 */
@Component({
  selector: 'wh-workflows-layout',
  imports: [RouterOutlet],
  template: `
    <div class="flex min-h-screen flex-col">
      <router-outlet />
    </div>
  `,
  styleUrl: './workflows-layout.component.scss'
})
export class WorkflowsLayoutComponent {}
