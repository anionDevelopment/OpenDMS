import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { OIDCProviderDTO, OIDCService, UserService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-login-form',
  standalone: false,
  templateUrl: './login-form.component.html',
  styleUrl: './login-form.component.scss'
})
export class LoginFormComponent implements OnInit {

  oidcProviders: OIDCProviderDTO[] = [];
  selectedProviderId: string | null = null;

  constructor(
    private userService: UserService,
    private oidcService: OIDCService,
    private userDataService: UserDataService,
    private router: Router,
    private storageService: StorageService,
  ) {}

  form: FormGroup = new FormGroup({
    username: new FormControl(''),
    password: new FormControl(''),
  });

  ngOnInit(): void {
    this.oidcService.aPIV3OIDCControllerGetOIDCProvidersGet().subscribe({
      next: (providers) => { this.oidcProviders = providers; },
      error: () => { this.oidcProviders = []; }
    });
  }

  public login(): void {
    const username: string = this.form.get('username')!.value;
    const password: string = this.form.get('password')!.value;
    this.userService.aPIV3UserControllerLoginPut(username, password)
      .pipe(
        switchMap((accessToken: any) => {
          this.storageService.setAccessToken(accessToken.value!);
          return this.userDataService.loadUserData();
        })
      ).subscribe(() => {
        this.router.navigate(['user', 'dashboard']);
      });
  }

  public loginWithOidc(): void {
    if (!this.selectedProviderId) {
      return;
    }
    const providerId = this.selectedProviderId;
    this.oidcService.aPIV3OIDCControllerInitiateOIDCLoginGet(providerId).subscribe((initiation) => {
      sessionStorage.setItem('oidcState', initiation.state!);
      sessionStorage.setItem('oidcProviderId', providerId);
      window.location.href = initiation.authorizationUrl!;
    });
  }
}
