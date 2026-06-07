import {
  ApplicationConfig,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { MessageService } from 'primeng/api';
import { authInterceptor } from './core/auth/auth.interceptor';
import { refreshInterceptor } from './core/auth/refresh.interceptor';
import { AuthService } from './core/auth/auth.service';
import { TokenStorage } from './core/auth/token-storage';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, refreshInterceptor])),
    MessageService,
    provideAppInitializer(() => {
      const auth = inject(AuthService);
      const tokens = inject(TokenStorage);

      if (!tokens.hasSession()) {
        return Promise.resolve();
      }

      return firstValueFrom(auth.restoreSession()).catch(() => undefined);
    }),
    provideAnimationsAsync(),
    providePrimeNG({
      theme: {
        preset: Aura,
        options: {
          // Keep PrimeNG generated styles in a dedicated cascade layer so
          // Tailwind utilities can reliably override component defaults.
          cssLayer: {
            name: 'primeng',
            order: 'theme, base, primeng'
          }
        }
      }
    })
  ]
};
