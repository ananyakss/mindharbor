import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = async (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  await authService.ensureInitialized();

  if (authService.isAuthenticated()) {
    return true;
  }

  authService.setPostAuthRedirect(state.url);
  return router.createUrlTree(['/auth']);
};

