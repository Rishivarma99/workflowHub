import { environment } from '../../../environments/environment';

export function authLog(message: string, detail?: unknown): void {
  if (environment.production) {
    return;
  }

  if (detail === undefined) {
    console.debug(`[Auth] ${message}`);
    return;
  }

  console.debug(`[Auth] ${message}`, detail);
}
