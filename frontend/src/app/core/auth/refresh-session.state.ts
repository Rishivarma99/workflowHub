import { BehaviorSubject } from 'rxjs';

let refreshing = false;
const refreshed$ = new BehaviorSubject<string | null>(null);

export function isRefreshInFlight(): boolean {
  return refreshing;
}

export function beginRefresh(): void {
  refreshing = true;
  refreshed$.next(null);
}

export function completeRefresh(accessToken: string): void {
  refreshing = false;
  refreshed$.next(accessToken);
}

export function failRefresh(): void {
  refreshing = false;
  refreshed$.next(null);
}

export function cancelInFlightRefresh(): void {
  refreshing = false;
  refreshed$.next(null);
}

export function waitForRefreshedAccessToken() {
  return refreshed$;
}
