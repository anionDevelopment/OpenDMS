import { Component, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { OpenDMSBackendService, StorageLocationDTO } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { UtilitiesService } from '../../../services/utilities.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-edit-storage-location-menu',
  standalone: false,
  templateUrl: './edit-storage-location-menu.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './edit-storage-location-menu.component.scss'
})
export class EditStorageLocationMenuComponent {

  @Input()
  userIsAllowedToAddStorageLocation: boolean = true;//TODO set initial value to false and set only to true when user has permission to do that


  @Output()
  storageLocationAdded: EventEmitter<StorageLocationDTO> = new EventEmitter<StorageLocationDTO>();

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private utilitiesService: UtilitiesService) {
  }

  addStorageLocation(): void {
    this.openDMSBackendService.aPIV3OpenDMSBackendAddStorageLocationNamePost("New storage-location", this.storageService.getAccessToken()).subscribe(newStorageLocationId => {
      this.openDMSBackendService.aPIV3OpenDMSBackendGetStorageLocationIdGet(newStorageLocationId, this.storageService.getAccessToken()).subscribe((newStorageLocation) => {
        this.storageLocationAdded.next(newStorageLocation);
      });
    });
  }
}
