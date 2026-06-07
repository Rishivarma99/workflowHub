import { Injectable } from '@angular/core';
import { authLog } from './auth-logger';

const TOKEN_SYNC_CHANNEL = 'workflowhub.auth.tokens';

interface TokenSyncMessage {
  type: 'tokens' | 'clear';
  access?: string;
  refresh?: string;
}

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  private readonly accessKey = 'workflowhub.auth.access';
  private readonly refreshKey = 'workflowhub.auth.refresh';
  private readonly channel =
    typeof BroadcastChannel !== 'undefined' ? new BroadcastChannel(TOKEN_SYNC_CHANNEL) : null;

  constructor() {
    this.channel?.addEventListener('message', (event: MessageEvent<TokenSyncMessage>) => {
      const message = event.data;
      if (!message?.type) {
        return;
      }

      if (message.type === 'tokens' && message.access && message.refresh) {
        this.persist(message.access, message.refresh);
        authLog('Tokens synced from another tab');
        return;
      }

      if (message.type === 'clear') {
        this.persist(null, null);
        authLog('Logout synced from another tab');
      }
    });
  }

  getAccess(): string | null {
    return sessionStorage.getItem(this.accessKey);
  }

  getRefresh(): string | null {
    return sessionStorage.getItem(this.refreshKey);
  }

  hasSession(): boolean {
    return !!this.getAccess() || !!this.getRefresh();
  }

  setTokens(accessToken: string, refreshToken: string): void {
    this.persist(accessToken, refreshToken);
    this.broadcast({ type: 'tokens', access: accessToken, refresh: refreshToken });
  }

  clear(): void {
    this.persist(null, null);
    this.broadcast({ type: 'clear' });
  }

  private persist(accessToken: string | null, refreshToken: string | null): void {
    if (accessToken) {
      sessionStorage.setItem(this.accessKey, accessToken);
    } else {
      sessionStorage.removeItem(this.accessKey);
    }

    if (refreshToken) {
      sessionStorage.setItem(this.refreshKey, refreshToken);
    } else {
      sessionStorage.removeItem(this.refreshKey);
    }
  }

  private broadcast(message: TokenSyncMessage): void {
    this.channel?.postMessage(message);
  }
}
