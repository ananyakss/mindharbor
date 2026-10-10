import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app-shell.component.html',
  styleUrls: ['./app-shell.component.css']
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly user = this.authService.currentUserSignal;

  protected readonly nav = [
    { path: '/dashboard', label: 'Dashboard', emoji: '🏠' },
    { path: '/mood', label: 'Mood Tracker', emoji: '😊' },
    { path: '/behavior', label: 'Behavior', emoji: '🧍' },
    { path: '/journal', label: 'Journal', emoji: '📓' },
    { path: '/therapy', label: 'Therapy', emoji: '🧠' },
    { path: '/resources', label: 'Resources', emoji: '📚' }
  ];

  protected logout(): void {
    this.authService
      .signOut()
      .pipe(finalize(() => this.router.navigateByUrl('/auth')))
      .subscribe({ error: () => this.router.navigateByUrl('/auth') });
  }
}
