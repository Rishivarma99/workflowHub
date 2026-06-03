import { Routes } from '@angular/router';

export const WORKFLOWS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layout/workflows-layout.component').then((m) => m.WorkflowsLayoutComponent),
    children: [
      { path: '', redirectTo: 'discover', pathMatch: 'full' },
      {
        path: 'discover',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPageComponent),
        data: { title: 'Discover', subtitle: 'Browse published workflows in the registry.' }
      },
      {
        path: 'search',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPageComponent),
        data: { title: 'Search', subtitle: 'Find workflows and components by capability.' }
      },
      {
        path: 'mine',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPageComponent),
        data: { title: 'My workflows', subtitle: 'Workflows you have published.' }
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPageComponent),
        data: { title: 'New workflow', subtitle: 'Publish a workflow from a public GitHub repo.' }
      },
      {
        path: 'settings',
        loadChildren: () =>
          import('./settings/settings.routes').then((m) => m.SETTINGS_ROUTES)
      }
    ]
  }
];
