import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { finalize, switchMap } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { GoogleIdentityService } from '../../../core/auth/google-identity.service';
import { WhIconComponent } from '../../../shared/ui/icon/wh-icon.component';
import { toUserFacingError } from '../../../shared/utils/api/to-user-facing-error';

@Component({
  selector: 'wh-login-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss'
})
export class LoginPageComponent {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly googleIdentity = inject(GoogleIdentityService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly features = [
    {
      icon: 'search' as const,
      title: 'Find by capability',
      description:
        'Search for a skill, hook, rule or MCP tool — get the exact workflow + file that has it.'
    },
    {
      icon: 'package' as const,
      title: 'Publish in minutes',
      description: 'Bundle your subagents, skills, hooks and rules, describe each, and share.'
    },
    {
      icon: 'shield-check' as const,
      title: 'Google sign-in',
      description: 'One-tap access with your Google account. No passwords.'
    }
  ];

  continueWithGoogle(): void {
    if (this.loading()) {
      return;
    }

    this.error.set(null);
    this.loading.set(true);

    this.googleIdentity
      .promptOrRender()
      .pipe(
        switchMap((idToken) => this.auth.loginWithGoogleIdToken(idToken)),
        takeUntilDestroyed(this.destroyRef),
        finalize(() => this.loading.set(false))
      )
      .subscribe({
        next: () => {
          void this.router.navigate(['/workflows/discover']);
        },
        error: (err) => {
          this.error.set(toUserFacingError(err));
        }
      });
  }
}
