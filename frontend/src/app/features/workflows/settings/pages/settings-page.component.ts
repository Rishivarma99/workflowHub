import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthApiService } from '../../../../core/auth/auth-api.service';
import { AuthService } from '../../../../core/auth/auth.service';
import { User } from '../../../../core/auth/auth.models';
import { NotificationService } from '../../../../core/services/notification.service';
import { WhAvatarComponent } from '../../../../shared/ui/avatar/wh-avatar.component';
import { WhIconComponent } from '../../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../../shared/utils/api/to-user-facing-error';
import { SettingsToggleComponent } from '../components/settings-toggle.component';
import { SettingsNotificationPrefsService } from '../state/settings-notification-prefs.service';

const USERNAME_PATTERN = /^[a-z0-9_]*$/;

/** Settings screen: logged-in user's profile and account preferences (design reference). */
@Component({
  selector: 'wh-settings-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, WhAvatarComponent, WhIconComponent, SettingsToggleComponent],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.scss'
})
export class SettingsPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly authApi = inject(AuthApiService);
  private readonly auth = inject(AuthService);
  private readonly notifications = inject(NotificationService);
  private readonly notifPrefs = inject(SettingsNotificationPrefsService);

  readonly loading = signal(true);
  readonly saving = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly saveError = signal<string | null>(null);
  readonly saved = signal(false);
  readonly user = signal<User | null>(null);

  readonly prefs = this.notifPrefs.prefs;

  readonly profileForm = new FormGroup({
    displayName: new FormControl('', {
      validators: [
        (ctrl) => {
          const v = (ctrl.value as string | null)?.trim() ?? '';
          if (!v) {
            return null;
          }
          return v.length >= 2 && v.length <= 50 ? null : { length: true };
        }
      ]
    }),
    username: new FormControl('', {
      validators: [
        (ctrl) => {
          const v = (ctrl.value as string | null)?.trim().toLowerCase() ?? '';
          if (!v) {
            return null;
          }
          return v.length >= 3 && v.length <= 30 && USERNAME_PATTERN.test(v) ? null : { username: true };
        }
      ]
    }),
    email: new FormControl({ value: '', disabled: true }),
    role: new FormControl('', { validators: [Validators.maxLength(80)] }),
    team: new FormControl('', { validators: [Validators.maxLength(80)] }),
    bio: new FormControl('', { validators: [Validators.maxLength(160)] })
  });

  readonly displayNameDraft = toSignal(this.profileForm.controls.displayName.valueChanges, {
    initialValue: ''
  });
  readonly usernameDraft = toSignal(this.profileForm.controls.username.valueChanges, {
    initialValue: ''
  });
  readonly roleDraft = toSignal(this.profileForm.controls.role.valueChanges, { initialValue: '' });
  readonly teamDraft = toSignal(this.profileForm.controls.team.valueChanges, { initialValue: '' });

  constructor() {
    const cached = this.auth.currentUser();
    if (cached) {
      this.user.set(cached);
      this.patchFormFromUser(cached);
      this.loading.set(false);
    }

    this.refreshProfile();
  }

  retryLoad(): void {
    this.loadError.set(null);
    this.loading.set(true);
    this.refreshProfile();
  }

  private refreshProfile(): void {
    this.authApi
      .getMe()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.auth.setCurrentUser(user);
          this.patchFormFromUser(user);
          this.loadError.set(null);
          this.loading.set(false);
        },
        error: (err) => {
          this.loadError.set(toUserFacingError(err));
          this.loading.set(false);
          if (!this.user()) {
            this.notifications.error(this.loadError() ?? 'Could not load your profile.');
          }
        }
      });
  }

  onSave(): void {
    if (this.profileForm.invalid || this.profileForm.pristine || this.saving()) {
      return;
    }

    const displayRaw = (this.profileForm.controls.displayName.value as string)?.trim() ?? '';
    const usernameRaw = (this.profileForm.controls.username.value as string)?.trim().toLowerCase() ?? '';
    const roleRaw = (this.profileForm.controls.role.value as string)?.trim() ?? '';
    const teamRaw = (this.profileForm.controls.team.value as string)?.trim() ?? '';
    const bioRaw = (this.profileForm.controls.bio.value as string)?.trim() ?? '';

    this.saving.set(true);
    this.saveError.set(null);
    this.saved.set(false);

    this.authApi
      .updateProfile({
        displayName: displayRaw.length ? displayRaw : null,
        username: usernameRaw.length ? usernameRaw : null,
        role: roleRaw.length ? roleRaw : null,
        team: teamRaw.length ? teamRaw : null,
        bio: bioRaw.length ? bioRaw : null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.user.set(updated);
          this.auth.setCurrentUser(updated);
          this.patchFormFromUser(updated);
          this.saving.set(false);
          this.saved.set(true);
          this.notifications.success('Profile saved.');
        },
        error: (err) => {
          this.saving.set(false);
          const message = toUserFacingError(err);
          this.saveError.set(message);
          this.notifications.error(message);
        }
      });
  }

  onReset(): void {
    const u = this.user();
    if (!u) {
      return;
    }
    this.patchFormFromUser(u);
    this.saved.set(false);
    this.saveError.set(null);
  }

  onSignOut(): void {
    this.auth
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        error: () => this.auth.hardLogout()
      });
  }

  togglePref(key: 'stars' | 'comments' | 'weekly' | 'autodraft'): void {
    this.notifPrefs.toggle(key);
  }

  profileDisplayName(user: User): string {
    const raw = (this.displayNameDraft() as string | null)?.trim() ?? '';
    if (raw) {
      return raw;
    }
    return user.displayName || user.name || '—';
  }

  chipRole(user: User): string {
    const raw = (this.roleDraft() as string | null)?.trim() ?? '';
    return raw || user.role || '—';
  }

  chipTeam(user: User): string {
    const raw = (this.teamDraft() as string | null)?.trim() ?? '';
    return raw || user.team || '—';
  }

  usernameHint(): string {
    const raw = (this.usernameDraft() as string | null)?.trim().toLowerCase() ?? '';
    return raw || 'username';
  }

  onUsernameInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const normalized = input.value.toLowerCase().replace(/[^a-z0-9_]/g, '');
    if (normalized !== input.value) {
      input.value = normalized;
    }
    this.profileForm.controls.username.setValue(normalized);
    this.profileForm.controls.username.markAsDirty();
  }

  joinedLabel(user: User): string {
    return new Date(user.createdAtUtc).toLocaleDateString(undefined, {
      month: 'long',
      year: 'numeric'
    });
  }

  private patchFormFromUser(user: User): void {
    this.profileForm.reset(
      {
        displayName: user.displayName ?? '',
        username: user.username ?? '',
        email: user.email,
        role: user.role ?? '',
        team: user.team ?? '',
        bio: user.bio ?? ''
      },
      { emitEvent: false }
    );
    this.profileForm.markAsPristine();
  }
}
