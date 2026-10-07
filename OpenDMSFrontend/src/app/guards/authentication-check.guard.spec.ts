import { TestBed } from '@angular/core/testing';
import { CanActivateFn, Router } from '@angular/router';
import { Observable, of } from 'rxjs';

import { authenticationCheckGuard } from './authentication-check.guard';
import { UserDataService } from '../services/user-data.service';

describe('authenticationCheckGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
      TestBed.runInInjectionContext(() => authenticationCheckGuard(...guardParameters));

  let userIsLoggedIn: boolean;
  let router: Router;

  beforeEach(() => {
    userIsLoggedIn = true;
    TestBed.configureTestingModule({
      providers: [
        {
          provide: UserDataService,
          useValue: {
            userIsLoggedIn: () => of(userIsLoggedIn),
          },
        },
      ],
    });
    router = TestBed.inject(Router);
    vi.spyOn(router, 'navigate');
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });

  it('should allow the navigation and not redirect when the user is logged in', () => {
    userIsLoggedIn = true;

    TestBed.runInInjectionContext(() => {
      (authenticationCheckGuard({} as never, {} as never) as Observable<boolean>).subscribe((result) => {
        expect(result).toBe(true);
        expect(router.navigate).not.toHaveBeenCalled();
      });
    });
  });

  it('should redirect to the start-page and reject the navigation when the user is not logged in', () => {
    userIsLoggedIn = false;

    TestBed.runInInjectionContext(() => {
      (authenticationCheckGuard({} as never, {} as never) as Observable<boolean>).subscribe((result) => {
        expect(result).toBe(false);
        expect(router.navigate).toHaveBeenCalledWith(['']);
      });
    });
  });
});
