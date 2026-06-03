import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { isApiRequest } from '../../shared/utils/api/is-api-request';
import { SKIP_AUTH } from './auth-context';
import { TokenStorage } from './token-storage';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_AUTH) || !isApiRequest(req.url, environment.apiBaseUrl)) {
    return next(req);
  }

  const access = inject(TokenStorage).getAccess();
  if (!access) {
    return next(req);
  }

  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${access}` }
    })
  );
};
