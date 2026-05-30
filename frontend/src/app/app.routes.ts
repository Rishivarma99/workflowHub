import { Routes } from '@angular/router';

/**
 * Top-level application routes.
 * - `home` is a temporary landing shell until real designs land.
 * - `auth` and the `workflows` business area are lazy-loaded route groups.
 * Guards (auth, etc.) will be attached to the business area once implemented.
 */
export const routes: Routes = [
  { path: '', redirectTo: 'home', pathMatch: 'full' },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.page').then((m) => m.HomePage)
  },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES)
  },
  {
    path: 'workflows',
    loadChildren: () =>
      import('./features/workflows/workflows.routes').then((m) => m.WORKFLOWS_ROUTES)
  },
  { path: '**', redirectTo: 'home' }
];
