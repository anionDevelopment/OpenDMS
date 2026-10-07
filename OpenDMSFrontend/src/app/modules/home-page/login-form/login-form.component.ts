import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AccessToken, OIDCProviderDTO, OIDCService, UserService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-login-form',
  standalone: false,
  templateUrl: './login-form.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
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

  /** Logs in with the username and password entered in the form, stores the returned access-token and navigates to the dashboard on success. */
  public login(): void {
    const username: string = this.form.get('username')!.value;
    const password: string = this.form.get('password')!.value;
    this.userService.aPIV3UserControllerLoginPut(username, password)
      .pipe(
        switchMap((accessToken: AccessToken) => {
          this.storageService.setAccessToken(accessToken.value!);
          return this.userDataService.loadUserData();
        })
      ).subscribe(() => {
        this.router.navigate(['user', 'dashboard']);
      });
  }

  /**
   * Starts the OIDC login for the selected provider: stores the expected state and the provider-id in
   * session-storage (checked by the callback-page against the state the identity-provider returns, to
   * detect a forged or replayed callback) and redirects the browser to the provider's authorization-URL.
   * Does nothing when no provider is selected.
   */
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
