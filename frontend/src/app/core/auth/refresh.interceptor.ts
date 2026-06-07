import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, filter, switchMap, take, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { isApiRequest } from '../../shared/utils/api/is-api-request';
import { authLog } from './auth-logger';
import { AuthService } from './auth.service';
import { SKIP_AUTH } from './auth-context';
import {
  beginRefresh,
  completeRefresh,
  failRefresh,
  isRefreshInFlight,
  waitForRefreshedAccessToken
} from './refresh-session.state';

export const refreshInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_AUTH) || !isApiRequest(req.url, environment.apiBaseUrl)) {
    return next(req);
  }

  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401) {
        return throwError(() => error);
      }

      const url = req.url;
      if (url.includes('/auth/refresh') || url.includes('/auth/external')) {
        authLog('Auth endpoint returned 401; logging out', url);
        auth.hardLogout();
        return throwError(() => error);
      }

      authLog('API returned 401; attempting token refresh', url);

      if (!isRefreshInFlight()) {
        beginRefresh();

        return auth.refresh().pipe(
          switchMap((pair) => {
            if (!pair.accessToken) {
              return throwError(() => new Error('Refresh returned empty access token'));
            }

            completeRefresh(pair.accessToken);
            authLog('Token refresh succeeded; retrying request', url);
            return retryAuthorized(req, pair.accessToken, next);
          }),
          catchError((refreshErr) => {
            failRefresh();
            authLog('Token refresh failed; logging out', refreshErr);
            auth.hardLogout();
            return throwError(() => refreshErr);
          })
        );
      }

      authLog('Waiting for in-flight token refresh', url);
      return waitForRefreshedAccessToken().pipe(
        filter((token): token is string => token !== null),
        take(1),
        switchMap((accessToken) => {
          authLog('In-flight refresh complete; retrying request', url);
          return retryAuthorized(req, accessToken, next);
        }),
        catchError((retryErr) => {
          if (retryErr instanceof HttpErrorResponse && retryErr.status === 401) {
            authLog('Retried request still unauthorized; logging out', url);
            auth.hardLogout();
          }

          return throwError(() => retryErr);
        })
      );
    })
  );
};

function retryAuthorized(
  req: Parameters<HttpInterceptorFn>[0],
  accessToken: string,
  next: Parameters<HttpInterceptorFn>[1]
) {
  return next(
    req.clone({
      setHeaders: { Authorization: `Bearer ${accessToken}` }
    })
  );
}
