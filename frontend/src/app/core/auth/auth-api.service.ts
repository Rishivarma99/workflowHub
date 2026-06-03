import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/types/api';
import { unwrapApiResponse } from '../../shared/utils/api/unwrap-api-response';
import {
  AuthResult,
  ExternalLoginRequest,
  GoogleLoginRequest,
  LogoutRequest,
  RefreshTokenRequest,
  TokenPair,
  UpdateProfileRequest,
  User
} from './auth.models';
import { skipAuth } from './auth-context';

@Injectable({ providedIn: 'root' })
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  loginWithExternal(provider: string, idToken: string): Observable<AuthResult> {
    const body: ExternalLoginRequest = { provider, idToken };
    return this.http
      .post<ApiResponse<AuthResult>>(`${this.base}/auth/external`, body, skipAuth())
      .pipe(map(unwrapApiResponse));
  }

  loginWithGoogle(idToken: string): Observable<AuthResult> {
    return this.loginWithExternal('google', idToken);
  }

  refresh(refreshToken: string): Observable<TokenPair> {
    const body: RefreshTokenRequest = { refreshToken };
    return this.http
      .post<ApiResponse<TokenPair>>(`${this.base}/auth/refresh`, body, skipAuth())
      .pipe(map(unwrapApiResponse));
  }

  logout(refreshToken: string | null): Observable<void> {
    const body: LogoutRequest = { refreshToken };
    return this.http
      .post<ApiResponse<null>>(`${this.base}/auth/logout`, body, skipAuth())
      .pipe(map(() => undefined));
  }

  getMe(): Observable<User> {
    return this.http
      .get<ApiResponse<User>>(`${this.base}/me`)
      .pipe(map(unwrapApiResponse));
  }

  updateProfile(body: UpdateProfileRequest): Observable<User> {
    return this.http
      .patch<ApiResponse<User>>(`${this.base}/me`, body)
      .pipe(map(unwrapApiResponse));
  }
}
