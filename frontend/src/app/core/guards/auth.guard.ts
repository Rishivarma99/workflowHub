import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { TokenStorage } from '../auth/token-storage';

export const authGuard: CanMatchFn = () => {
  const auth = inject(AuthService);
  const tokens = inject(TokenStorage);
  const router = inject(Router);

  if (!tokens.getAccess()) {
    return router.createUrlTree(['/auth/login']);
  }

  if (auth.currentUser()) {
    return true;
  }

  return auth.loadCurrentUser().pipe(
    map(() => true),
    catchError(() => of(router.createUrlTree(['/auth/login'])))
  );
};
