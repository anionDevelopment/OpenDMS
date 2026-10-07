import { TestBed } from '@angular/core/testing';

import { StorageService } from './storage.service';

describe('StorageService', () => {
  let service: StorageService;

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(StorageService);
  });

  afterEach(() => {
    sessionStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should return the access-token which was set before', () => {
    service.setAccessToken('token1');

    expect(service.getAccessToken()).toBe('token1');
  });

  it('should throw when the access-token was never set', () => {
    expect(() => service.getAccessToken()).toThrowError('AccessToken is not available.');
  });

  it('should remove the access-token when it is set to null', () => {
    service.setAccessToken('token1');

    service.setAccessToken(null);

    expect(service.hasAccessToken()).toBe(false);
  });

  it('should report hasAccessToken as true only while a token is stored', () => {
    expect(service.hasAccessToken()).toBe(false);

    service.setAccessToken('token1');

    expect(service.hasAccessToken()).toBe(true);
  });

  it('should remove the access-token via removeAccessToken', () => {
    service.setAccessToken('token1');

    service.removeAccessToken();

    expect(service.hasAccessToken()).toBe(false);
  });

  it('should return the user-name which was set before', () => {
    service.setUserName('Alice');

    expect(service.getUserName()).toBe('Alice');
  });

  it('should throw when the user-name was never set', () => {
    expect(() => service.getUserName()).toThrowError('UserName is not available.');
  });

  it('should remove the user-name when it is set to undefined', () => {
    service.setUserName('Alice');

    service.setUserName(undefined);

    expect(() => service.getUserName()).toThrowError('UserName is not available.');
  });

  it('should return the user-id which was set before', () => {
    service.setUserId('user1');

    expect(service.getUserId()).toBe('user1');
  });

  it('should throw when the user-id was never set', () => {
    expect(() => service.getUserId()).toThrowError('UserId is not available.');
  });

  it('should return true when userIsAdmin was set to true', () => {
    service.setUserIsAdmin(true);

    expect(service.getUserIsAdmin()).toBe(true);
  });

  it('should return false when userIsAdmin was set to false', () => {
    service.setUserIsAdmin(false);

    expect(service.getUserIsAdmin()).toBe(false);
  });

  it('should throw when userIsAdmin was never set', () => {
    expect(() => service.getUserIsAdmin()).toThrowError('UserIsAdmin is not available.');
  });

  it('should remove userIsAdmin when it is set to null', () => {
    service.setUserIsAdmin(true);

    service.setUserIsAdmin(null);

    expect(() => service.getUserIsAdmin()).toThrowError('UserIsAdmin is not available.');
  });
});
