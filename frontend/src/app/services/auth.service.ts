import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../environments/environment';
import { AuthResponse, SignInPayload, SignUpPayload } from '../models/auth.model';
import { User } from '../models/user.model';
import { catchError, mapTo, of, tap } from 'rxjs';

const TOKEN_KEY = 'mha.accessToken';
const USER_KEY = 'mha.currentUser';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);

  private readonly accessTokenSignal = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  private readonly userSignal = signal<User | null>(this.readStoredUser());
  private initialized = false;
  private initializePromise: Promise<void> | null = null;
  private redirectAfterAuth: string | null = null;

  readonly isAuthenticatedSignal = computed(() => !!this.accessTokenSignal());
  readonly currentUserSignal = computed(() => this.userSignal());

  signUp(payload: SignUpPayload) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/signup`, payload, { withCredentials: true })
      .pipe(tap((response) => this.handleAuthSuccess(response)));
  }

  signIn(payload: SignInPayload) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/signin`, payload, { withCredentials: true })
      .pipe(tap((response) => this.handleAuthSuccess(response)));
  }

  refresh() {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/auth/refresh`, {}, { withCredentials: true })
      .pipe(
        tap((response) => this.handleAuthSuccess(response)),
        mapTo(true),
        catchError(() => {
          this.clearAuth();
          return of(false);
        })
      );
  }

  signOut() {
    return this.http
      .post<void>(`${environment.apiUrl}/auth/signout`, {}, { withCredentials: true })
      .pipe(
        tap(() => this.clearAuth()),
        catchError(() => {
          this.clearAuth();
          return of(void 0);
        })
      );
  }

  async ensureInitialized() {
    if (this.initialized) {
      return;
    }

    if (!this.initializePromise) {
      this.initializePromise = new Promise<void>((resolve) => {
        this.refresh().subscribe({
          next: () => {
            this.initialized = true;
            resolve();
          },
          error: () => {
            this.initialized = true;
            resolve();
          },
          complete: () => {
            this.initialized = true;
            resolve();
          }
        });
      });
    }

    await this.initializePromise;
  }

  get accessToken(): string | null {
    return this.accessTokenSignal();
  }

  get currentUser(): User | null {
    return this.userSignal();
  }

  isAuthenticated(): boolean {
    return this.isAuthenticatedSignal();
  }

  setPostAuthRedirect(url: string) {
    this.redirectAfterAuth = url;
  }

  consumePostAuthRedirect(): string | null {
    const redirect = this.redirectAfterAuth;
    this.redirectAfterAuth = null;
    return redirect;
  }

  private handleAuthSuccess(response: AuthResponse) {
    this.accessTokenSignal.set(response.accessToken);
    this.userSignal.set(response.user);

    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
  }

  private clearAuth() {
    this.accessTokenSignal.set(null);
    this.userSignal.set(null);

    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  }

  private readStoredUser(): User | null {
    try {
      const stored = localStorage.getItem(USER_KEY);
      if (!stored) return null;
      const raw = JSON.parse(stored) as Partial<User>;
      if (!raw.id || !raw.username || !raw.email) return null;
      return {
        id: raw.id,
        username: raw.username,
        email: raw.email,
        loginStreak: raw.loginStreak ?? 0,
        gamificationPoints: raw.gamificationPoints ?? 0
      };
    } catch {
      return null;
    }
  }
}

