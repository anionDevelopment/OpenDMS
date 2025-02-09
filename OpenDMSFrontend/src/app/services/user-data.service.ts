import { Injectable } from '@angular/core';
import { StorageService } from './storage.service';
import { Observable, first, map, mergeMap, of, switchMap, tap } from 'rxjs';
import { UserInformationDTO, UserService } from '../generated/open-dms-backend';

@Injectable({
  providedIn: 'root'
})
export class UserDataService {
  private loaded: boolean = false;

  constructor(private userService: UserService, private storageService: StorageService) {
  }

  loadUserData(): Observable<void> {
    return this.ensureLoaded();
  }

  unloadUserData() {
    this.storageService.setUserName(null);
    this.storageService.setUserId(null);
    this.storageService.setAccessToken(null);
    this.storageService.setUserIsAdmin(false);
  }

  getUserId(): Observable<string> {
    return this.ensureLoaded().pipe(map(() => {
      return this.storageService.getUserId();
    }));
  }

  getUserName(): Observable<string> {
    return this.ensureLoaded().pipe(map(() => {
      return this.storageService.getUserName();
    }));
  }

  userIsLoggedIn(): Observable<boolean> {
    if (this.storageService.hasAccessToken()) {
      return this.userService.aPIV1UserControllerTokenIsValidGet(this.storageService.getAccessToken());
    } else {
      return of(false);
    }
  }

  userIsAdmin(): Observable<boolean> {
    return this.ensureLoaded().pipe(map(() => {
      try {
        return this.storageService.getUserIsAdmin();
      } catch {
        return false;
      }
    }));
  }

  ensureLoaded(): Observable<void> {
    let pipe: Observable<any>;
    if (this.loaded) {
      pipe = of(null);//null is required here because otherwise first() will throw a 'no elements in sequence'-error
    } else {
      pipe = this.userService.aPIV1UserControllerGetUserInformationGet(this.storageService.getAccessToken()).pipe(
        tap((value: UserInformationDTO) => {
          this.storageService.setUserName(value.name);
          this.storageService.setUserId(value.id);
          this.storageService.setUserIsAdmin(value.isAdmin);
          this.loaded = true;
        }
        ));
    }
    return pipe.pipe(first());
  }
}
