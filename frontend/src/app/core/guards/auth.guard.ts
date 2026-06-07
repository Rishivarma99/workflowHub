import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenStorage } from '../auth/token-storage';
import { authLog } from '../auth/auth-logger';

export const authGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const tokens = inject(TokenStorage);
  const router = inject(Router);

  if (!tokens.hasSession()) {
    authLog('Auth guard blocked: no tokens');
    return router.createUrlTree(['/auth/login']);
  }

  if (auth.currentUser()) {
    return true;
  }

  authLog('Auth guard restoring session');
  return auth.restoreSession().pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/auth/login'])))
  );
};
