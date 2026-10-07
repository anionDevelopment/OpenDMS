import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { OIDCService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { UserDataService } from '../../../services/user-data.service';
import { switchMap } from 'rxjs';

@Component({
  selector: 'app-oidc-callback',
  standalone: false,
  templateUrl: './oidc-callback.component.html',
  styleUrl: './oidc-callback.component.scss'
})
export class OidcCallbackComponent implements OnInit {

  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private oidcService: OIDCService,
    private storageService: StorageService,
    private userDataService: UserDataService,
  ) {}

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const code = params.get('code');
    const state = params.get('state');
    const storedState = sessionStorage.getItem('oidcState');
    const providerId = sessionStorage.getItem('oidcProviderId');

    if (!code || !state || !storedState || !providerId) {
      this.error = $localize`:@@oidc_callback_missing_params:Missing OIDC callback parameters.`;
      return;
    }

    if (state !== storedState) {
      this.error = $localize`:@@oidc_callback_state_mismatch:Invalid OIDC state. Please try logging in again.`;
      return;
    }

    sessionStorage.removeItem('oidcState');
    sessionStorage.removeItem('oidcProviderId');

    this.oidcService.aPIV3OIDCControllerExchangeOIDCCodePost({ providerId, code, state })
      .pipe(
        switchMap((accessToken) => {
          this.storageService.setAccessToken(accessToken.value!);
          return this.userDataService.loadUserData();
        })
      )
      .subscribe({
        next: () => this.router.navigate(['user', 'dashboard']),
        error: () => {
          this.error = $localize`:@@oidc_callback_exchange_failed:Login failed. Please try again.`;
        }
      });
  }
}
