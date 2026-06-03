import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

/**
 * Top-level routes: auth (public) and workflows business area (shell + features).
 */
export const routes: Routes = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./features/auth/auth.routes').then((m) => m.AUTH_ROUTES)
  },
  {
    path: 'workflows',
    canMatch: [authGuard],
    loadChildren: () =>
      import('./features/workflows/workflows.routes').then((m) => m.WORKFLOWS_ROUTES)
  },
  { path: 'home', redirectTo: 'auth/login', pathMatch: 'full' },
  { path: '**', redirectTo: 'auth/login' }
];
