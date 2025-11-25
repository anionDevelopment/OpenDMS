import { Component, OnInit } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { OpenDMSBackendService, StorageLocationDTO } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-content-tree',
  standalone: false,
  templateUrl: './content-tree.component.html',
  styleUrl: './content-tree.component.scss'
})
export class ContentTreeComponent implements OnInit {
  storageLocations: StorageLocationDTO[] = [];

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  ngOnInit(): void {
    this.openDMSBackendService.aPIV2OpenDMSBackendGetAllViewableStorageLocationsGet(this.storageService.getAccessToken()).subscribe((storageLocations) => {
      storageLocations.forEach(storageLocation => {
        this.storageLocations.push(storageLocation);
      });
    });
  }

  onContainerRemoved(containerId: string) {
    var storageLocations = this.storageLocations.filter(storageLocation => storageLocation.id != containerId);
    this.storageLocations = [...storageLocations];
  }
  
  onStorageLocationAdded(storageLocation: StorageLocationDTO){
    var storageLocations = this.storageLocations
    storageLocations.push(storageLocation);
    this.storageLocations = [...storageLocations];
  }
}
