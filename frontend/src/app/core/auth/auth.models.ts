export interface User {
  id: string;
  email: string;
  name: string;
  displayName: string | null;
  username: string | null;
  role: string | null;
  team: string | null;
  bio: string | null;
  avatarUrl: string | null;
  createdAtUtc: string;
}

export interface TokenPair {
  accessToken: string;
  refreshToken: string;
}

export interface AuthResult {
  tokens: TokenPair;
  user: User;
}

export interface GoogleLoginRequest {
  idToken: string;
}

export interface ExternalLoginRequest {
  provider: string;
  idToken: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface LogoutRequest {
  refreshToken: string | null;
}

export interface UpdateProfileRequest {
  displayName: string | null;
  username: string | null;
  role: string | null;
  team: string | null;
  bio: string | null;
}
