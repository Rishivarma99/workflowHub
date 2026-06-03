import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, filter, finalize, share, switchMap, take, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiRequest } from '../../shared/utils/api/is-api-request';
import { AuthService } from './auth.service';
import { SKIP_AUTH } from './auth-context';

let refreshInFlight: ReturnType<AuthService['refresh']> | null = null;

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_AUTH) || !isApiRequest(req.url, environment.apiBaseUrl)) {
    return next(req);
  }

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      const url = req.url;
      if (url.includes('/auth/refresh') || url.includes('/auth/external')) {
        inject(AuthService).hardLogout();
        return throwError(() => error);
      }

      const auth = inject(AuthService);

      if (!refreshInFlight) {
        refreshInFlight = auth.refresh().pipe(
          finalize(() => {
            refreshInFlight = null;
          }),
          share()
        );
      }

      return refreshInFlight.pipe(
        take(1),
        filter((pair) => !!pair.accessToken),
        switchMap((pair) =>
          next(
            req.clone({
              setHeaders: { Authorization: `Bearer ${pair.accessToken}` }
            })
          )
        ),
        catchError((refreshErr) => {
          auth.hardLogout();
          return throwError(() => refreshErr);
        })
      );
    })
  );
};
