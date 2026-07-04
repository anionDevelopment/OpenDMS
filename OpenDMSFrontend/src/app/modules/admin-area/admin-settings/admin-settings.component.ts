import { Component } from '@angular/core';
import { GeneralSettingsDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './admin-settings.component.html',
  styleUrl: './admin-settings.component.scss'
})
export class AdminSettingsComponent {
  autoGenerateAISummary: boolean = false;

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
    this.openDMSBackendService.aPIV3OpenDMSBackendGetGeneralSettingsGet(this.storageService.getAccessToken()).subscribe(settings => {
      this.autoGenerateAISummary = settings.autoGenerateAISummary ?? false;
    });
  }

  onAutoGenerateAISummaryChanged(enabled: boolean): void {
    this.autoGenerateAISummary = enabled;
    const settings: GeneralSettingsDTO = { autoGenerateAISummary: enabled };
    this.openDMSBackendService.aPIV3OpenDMSBackendSetGeneralSettingsPut(this.storageService.getAccessToken(), settings).subscribe();
  }
}
