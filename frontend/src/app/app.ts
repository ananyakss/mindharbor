import { Component, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`
})
export class AppComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  async ngOnInit(): Promise<void> {
    await this.authService.ensureInitialized();
    const path = this.router.url.split('?')[0];

    if (this.authService.isAuthenticated() && (path === '/' || path === '' || path === '/auth')) {
      await this.router.navigateByUrl('/dashboard');
    }
  }
}
