import { Routes } from '@angular/router';

/**
 * Business area routes. The layout component wraps all child features.
 * Child feature pages are lazy-loaded as standalone components.
 * Add child features (e.g. workflows list, runs, settings) here as designs land.
 */
export const WORKFLOWS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/workflows-layout.component').then((m) => m.WorkflowsLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.page').then((m) => m.DashboardPage)
      }
    ]
  }
];
