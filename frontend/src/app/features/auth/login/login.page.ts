import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { WhIconComponent } from '../../../shared/ui/icon/wh-icon.component';

/**
 * Google sign-in screen. Auth wiring (GIS + POST /auth/google) lands in a follow-up;
 * Continue temporarily enters the authenticated shell for UI development.
 */
@Component({
  selector: 'wh-login-page',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [WhIconComponent],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss'
})
export class LoginPage {
  private readonly router = inject(Router);

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
    // TODO(auth): Google Identity Services → POST /auth/google → store tokens
    void this.router.navigate(['/workflows/discover']);
  }
}
