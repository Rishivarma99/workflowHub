import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

declare global {
  interface Window {
    google?: {
      accounts: {
        id: {
          initialize(config: {
            client_id: string;
            callback: (response: { credential: string }) => void;
            auto_select?: boolean;
          }): void;
          prompt(
            notification?: (notification: {
              isNotDisplayed: () => boolean;
              isSkippedMoment: () => boolean;
            }) => void
          ): void;
          cancel?(): void;
          disableAutoSelect?(): void;
          renderButton(
            parent: HTMLElement,
            options: { theme?: string; size?: string; width?: number; text?: string }
          ): void;
        };
      };
    };
  }
}

type CredentialHandler = {
  complete: (credential: string) => void;
  fail: (message: string) => void;
};

/**
 * Wraps Google Identity Services. GIS allows one initialize() per page load — we register
 * once and route each sign-in attempt to the active Observable subscriber.
 */
@Injectable({ providedIn: 'root' })
export class GoogleIdentityService {
  private gisInitialized = false;
  private activeHandler: CredentialHandler | null = null;

  /** Clears GIS UI state after logout so the next sign-in can prompt again. */
  resetSession(): void {
    this.activeHandler = null;
    window.google?.accounts?.id?.cancel?.();
    window.google?.accounts?.id?.disableAutoSelect?.();
  }

  promptOrRender(buttonHost?: HTMLElement): Observable<string> {
    return new Observable((subscriber) => {
      const clientId = environment.googleClientId;
      if (!clientId) {
        subscriber.error(new Error('Google client id is not configured.'));
        return;
      }

      let settled = false;
      const complete = (credential: string) => {
        if (settled) {
          return;
        }
        settled = true;
        this.activeHandler = null;
        subscriber.next(credential);
        subscriber.complete();
      };
      const fail = (message: string) => {
        if (settled) {
          return;
        }
        settled = true;
        this.activeHandler = null;
        subscriber.error(new Error(message));
      };

      let subscriptionHandler: CredentialHandler | null = null;

      const run = () => {
        if (!window.google?.accounts?.id) {
          fail('Google Identity Services failed to load.');
          return;
        }

        subscriptionHandler = { complete, fail };
        this.activeHandler = subscriptionHandler;
        this.ensureGisInitialized(clientId);
        window.google.accounts.id.cancel?.();

        if (buttonHost) {
          window.google.accounts.id.renderButton(buttonHost, {
            theme: 'outline',
            size: 'large',
            text: 'continue_with',
            width: 320
          });
          return;
        }

        window.google.accounts.id.prompt((notification) => {
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            fail(
              'Google sign-in was not shown. Close any blocker, then click Continue with Google again.'
            );
          }
        });
      };

      if (window.google?.accounts?.id) {
        run();
      } else {
        const interval = window.setInterval(() => {
          if (window.google?.accounts?.id) {
            window.clearInterval(interval);
            run();
          }
        }, 100);

        window.setTimeout(() => {
          window.clearInterval(interval);
          if (!window.google?.accounts?.id && !settled) {
            fail('Google Identity Services timed out.');
          }
        }, 10000);
      }

      return () => {
        if (subscriptionHandler && this.activeHandler === subscriptionHandler) {
          this.activeHandler = null;
        }
        window.google?.accounts?.id?.cancel?.();
      };
    });
  }

  private ensureGisInitialized(clientId: string): void {
    if (this.gisInitialized) {
      return;
    }

    window.google!.accounts.id.initialize({
      client_id: clientId,
      auto_select: false,
      callback: (response) => {
        const handler = this.activeHandler;
        if (!handler) {
          return;
        }
        if (response.credential) {
          handler.complete(response.credential);
        } else {
          handler.fail('No credential returned.');
        }
      }
    });
    this.gisInitialized = true;
  }
}
