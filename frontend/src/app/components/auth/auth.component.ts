import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { finalize } from 'rxjs';

type AuthMode = 'signin' | 'signup';

@Component({
  selector: 'app-auth',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  protected mode = signal<AuthMode>('signin');
  protected loading = signal(false);
  protected error = signal<string | null>(null);

  protected authForm = this.fb.group({
    username: ['', []],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor() {
    this.applyUsernameValidators('signin');
  }

  toggleMode(nextMode: AuthMode) {
    this.mode.set(nextMode);
    this.error.set(null);
    this.applyUsernameValidators(nextMode);
  }

  submit() {
    if (this.authForm.invalid) {
      this.authForm.markAllAsTouched();
      return;
    }

    const { username, email, password } = this.authForm.getRawValue();
    this.loading.set(true);
    this.error.set(null);

    const request$ =
      this.mode() === 'signin'
        ? this.authService.signIn({ email: email!, password: password! })
        : this.authService.signUp({ username: username!, email: email!, password: password! });

    request$
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => {
          const redirect = this.authService.consumePostAuthRedirect() ?? '/dashboard';
          this.router.navigateByUrl(redirect);
        },
        error: (err) => {
          const message = err?.error?.message ?? 'Unable to complete the request. Please try again.';
          this.error.set(message);
        }
      });
  }

  private applyUsernameValidators(mode: AuthMode) {
    const usernameControl = this.authForm.controls.username;
    if (mode === 'signup') {
      usernameControl.setValidators([Validators.required, Validators.minLength(3)]);
    } else {
      usernameControl.clearValidators();
      usernameControl.reset('');
    }
    usernameControl.updateValueAndValidity();
  }
}

