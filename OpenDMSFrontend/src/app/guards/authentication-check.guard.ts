import { CanActivateFn } from '@angular/router';

export const authenticationCheckGuard: CanActivateFn = (route, state) => {
  return true;
};
