import { Injectable, signal } from '@angular/core';

export interface NotificationPrefs {
  stars: boolean;
  comments: boolean;
  weekly: boolean;
  autodraft: boolean;
}

const STORAGE_KEY = 'wh.settings.notification-prefs';

const DEFAULT_PREFS: NotificationPrefs = {
  stars: true,
  comments: true,
  weekly: false,
  autodraft: true
};

@Injectable({ providedIn: 'root' })
export class SettingsNotificationPrefsService {
  readonly prefs = signal<NotificationPrefs>(this.load());

  toggle(key: keyof NotificationPrefs): void {
    const next = { ...this.prefs(), [key]: !this.prefs()[key] };
    this.prefs.set(next);
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    } catch {
      /* ignore quota / private mode */
    }
  }

  private load(): NotificationPrefs {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return { ...DEFAULT_PREFS };
      }
      const parsed = JSON.parse(raw) as Partial<NotificationPrefs>;
      return { ...DEFAULT_PREFS, ...parsed };
    } catch {
      return { ...DEFAULT_PREFS };
    }
  }
}
