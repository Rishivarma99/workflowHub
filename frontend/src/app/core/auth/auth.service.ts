import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, switchMap, tap, throwError } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { GoogleIdentityService } from './google-identity.service';
import { TokenPair, User } from './auth.models';
import { TokenStorage } from './token-storage';
import { authLog } from './auth-logger';
import { cancelInFlightRefresh } from './refresh-session.state';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(AuthApiService);
  private readonly tokens = inject(TokenStorage);
  private readonly googleIdentity = inject(GoogleIdentityService);
  private readonly router = inject(Router);

  readonly currentUser = signal<User | null>(null);
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  loginWithGoogleIdToken(idToken: string): Observable<User> {
    return this.api.loginWithGoogle(idToken).pipe(
      tap((result) => {
        this.tokens.setTokens(result.tokens.accessToken, result.tokens.refreshToken);
        this.currentUser.set(result.user);
        authLog('Login succeeded');
      }),
      map((result) => result.user)
    );
  }

  restoreSession(): Observable<User> {
    if (!this.tokens.hasSession()) {
      authLog('No stored session');
      return throwError(() => new Error('No session'));
    }

    const loadProfile$ = this.api.getMe().pipe(tap((user) => this.currentUser.set(user)));

    if (this.tokens.getAccess()) {
      authLog('Restoring session from access token');
      return loadProfile$;
    }

    authLog('Access token missing; refreshing before restore');
    return this.refresh().pipe(switchMap(() => loadProfile$));
  }

  loadCurrentUser(): Observable<User> {
    return this.restoreSession();
  }

  refresh(): Observable<TokenPair> {
    const refreshToken = this.tokens.getRefresh();
    if (!refreshToken) {
      authLog('Refresh aborted: no refresh token in storage');
      return throwError(() => new Error('No refresh token'));
    }

    authLog('Calling /auth/refresh');
    return this.api.refresh(refreshToken).pipe(
      tap((pair) => {
        this.tokens.setTokens(pair.accessToken, pair.refreshToken);
        authLog('Refresh token rotated; new tokens stored');
      })
    );
  }

  logout(): Observable<void> {
    const refresh = this.tokens.getRefresh();
    return this.api.logout(refresh).pipe(
      catchError(() => of(undefined)),
      tap(() => this.hardLogout())
    );
  }

  hardLogout(): void {
    authLog('Hard logout');
    cancelInFlightRefresh();
    this.googleIdentity.resetSession();
    this.tokens.clear();
    this.currentUser.set(null);
    void this.router.navigate(['/auth/login']);
  }

  setCurrentUser(user: User): void {
    this.currentUser.set(user);
  }
}
