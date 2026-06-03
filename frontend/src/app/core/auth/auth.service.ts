import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, catchError, map, of, tap, throwError } from 'rxjs';
import { AuthApiService } from './auth-api.service';
import { GoogleIdentityService } from './google-identity.service';
import { TokenPair, User } from './auth.models';
import { TokenStorage } from './token-storage';

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
      }),
      map((result) => result.user)
    );
  }

  loadCurrentUser(): Observable<User> {
    if (!this.tokens.getAccess()) {
      return throwError(() => new Error('No access token'));
    }

    return this.api.getMe().pipe(
      tap((user) => this.currentUser.set(user)),
      catchError((err) => {
        this.hardLogout();
        return throwError(() => err);
      })
    );
  }

  refresh(): Observable<TokenPair> {
    const refreshToken = this.tokens.getRefresh();
    if (!refreshToken) {
      return throwError(() => new Error('No refresh token'));
    }

    return this.api.refresh(refreshToken).pipe(
      tap((pair) => this.tokens.setTokens(pair.accessToken, pair.refreshToken))
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
    this.googleIdentity.resetSession();
    this.tokens.clear();
    this.currentUser.set(null);
    void this.router.navigate(['/auth/login']);
  }

  setCurrentUser(user: User): void {
    this.currentUser.set(user);
  }
}
