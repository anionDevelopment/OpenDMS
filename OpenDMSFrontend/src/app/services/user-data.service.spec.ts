import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import type { Mock } from 'vitest';

import { UserDataService } from './user-data.service';
import { UserService, UserInformationDTO } from '../generated/open-dms-backend';
import { StorageService } from './storage.service';
import { ThemeService } from './theme.service';

interface UserServiceMock {
  aPIV3UserControllerGetUserInformationGet: Mock;
  aPIV3UserControllerTokenIsValidGet: Mock;
}

interface StorageServiceMock {
  getAccessToken: Mock;
  hasAccessToken: Mock;
  setUserName: Mock;
  setUserId: Mock;
  setUserIsAdmin: Mock;
  setAccessToken: Mock;
  getUserId: Mock;
  getUserName: Mock;
  getUserIsAdmin: Mock;
}

describe('UserDataService', () => {
  let service: UserDataService;
  let userServiceSpy: UserServiceMock;
  let storageServiceSpy: StorageServiceMock;
  let themeServiceSpy: { unloadModeOfUser: Mock };
  const userInformation: UserInformationDTO = { id: 'user1', name: 'Alice', isAdmin: true };

  beforeEach(() => {
    userServiceSpy = {
      aPIV3UserControllerGetUserInformationGet: vi.fn().mockReturnValue(of(userInformation)),
      aPIV3UserControllerTokenIsValidGet: vi.fn().mockReturnValue(of(true)),
    };
    storageServiceSpy = {
      getAccessToken: vi.fn().mockReturnValue('accesstoken1'),
      hasAccessToken: vi.fn().mockReturnValue(true),
      setUserName: vi.fn(),
      setUserId: vi.fn(),
      setUserIsAdmin: vi.fn(),
      setAccessToken: vi.fn(),
      getUserId: vi.fn().mockReturnValue('user1'),
      getUserName: vi.fn().mockReturnValue('Alice'),
      getUserIsAdmin: vi.fn().mockReturnValue(true),
    };
    themeServiceSpy = {
      unloadModeOfUser: vi.fn(),
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: UserService, useValue: userServiceSpy },
        { provide: StorageService, useValue: storageServiceSpy },
        { provide: ThemeService, useValue: themeServiceSpy },
      ],
    });
    service = TestBed.inject(UserDataService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch the user-information from the backend only once and cache it for later calls', () => {
    service.getUserId().subscribe();
    service.getUserName().subscribe();

    expect(userServiceSpy.aPIV3UserControllerGetUserInformationGet).toHaveBeenCalledTimes(1);
  });

  it('should store the fetched user-information via the storage-service', () => {
    service.getUserId().subscribe();

    expect(storageServiceSpy.setUserName).toHaveBeenCalledWith(userInformation.name);
    expect(storageServiceSpy.setUserId).toHaveBeenCalledWith(userInformation.id);
    expect(storageServiceSpy.setUserIsAdmin).toHaveBeenCalledWith(userInformation.isAdmin);
  });

  it('should resolve getUserId with the id returned from the storage-service', () => {
    service.getUserId().subscribe((id) => {
      expect(id).toBe('user1');
    });
  });

  it('should resolve getUserName with the name returned from the storage-service', () => {
    service.getUserName().subscribe((name) => {
      expect(name).toBe('Alice');
    });
  });

  it('should resolve userIsAdmin with true when the cached admin-flag is true', () => {
    storageServiceSpy.getUserIsAdmin.mockReturnValue(true);

    service.userIsAdmin().subscribe((isAdmin) => {
      expect(isAdmin).toBe(true);
    });
  });

  it('should resolve userIsAdmin with false without throwing when the admin-flag is not available yet', () => {
    storageServiceSpy.getUserIsAdmin.mockImplementation(() => {
      throw new Error('UserIsAdmin is not available.');
    });

    service.userIsAdmin().subscribe((isAdmin) => {
      expect(isAdmin).toBe(false);
    });
  });

  it('should resolve userIsLoggedIn with false without calling the backend when no access-token is stored', () => {
    storageServiceSpy.hasAccessToken.mockReturnValue(false);

    service.userIsLoggedIn().subscribe((isLoggedIn) => {
      expect(isLoggedIn).toBe(false);
    });
    expect(userServiceSpy.aPIV3UserControllerTokenIsValidGet).not.toHaveBeenCalled();
  });

  it('should ask the backend whether the stored access-token is valid when one is stored', () => {
    storageServiceSpy.hasAccessToken.mockReturnValue(true);
    storageServiceSpy.getAccessToken.mockReturnValue('accesstoken1');

    service.userIsLoggedIn().subscribe((isLoggedIn) => {
      expect(isLoggedIn).toBe(true);
    });
    expect(userServiceSpy.aPIV3UserControllerTokenIsValidGet).toHaveBeenCalledWith('accesstoken1');
  });

  it('should clear the stored user-data and the theme when unloading the user', () => {
    service.unloadUserData();

    expect(storageServiceSpy.setUserName).toHaveBeenCalledWith(null);
    expect(storageServiceSpy.setUserId).toHaveBeenCalledWith(null);
    expect(storageServiceSpy.setAccessToken).toHaveBeenCalledWith(null);
    expect(storageServiceSpy.setUserIsAdmin).toHaveBeenCalledWith(false);
    expect(themeServiceSpy.unloadModeOfUser).toHaveBeenCalled();
  });

  it('should fetch the user-information again after it was unloaded', () => {
    service.getUserId().subscribe();
    service.unloadUserData();

    service.getUserId().subscribe();

    expect(userServiceSpy.aPIV3UserControllerGetUserInformationGet).toHaveBeenCalledTimes(2);
  });
});
