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
          import('./shared/placeholder.page').then((m) => m.PlaceholderPage),
        data: { title: 'Discover', subtitle: 'Browse published workflows in the registry.' }
      },
      {
        path: 'search',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPage),
        data: { title: 'Search', subtitle: 'Find workflows and components by capability.' }
      },
      {
        path: 'mine',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPage),
        data: { title: 'My workflows', subtitle: 'Workflows you have published.' }
      },
      {
        path: 'create',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPage),
        data: { title: 'New workflow', subtitle: 'Publish a workflow from a public GitHub repo.' }
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./shared/placeholder.page').then((m) => m.PlaceholderPage),
        data: { title: 'Settings', subtitle: 'Account and profile — user management coming next.' }
      }
    ]
  }
];
