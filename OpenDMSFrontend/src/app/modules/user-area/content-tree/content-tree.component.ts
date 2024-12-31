import { Component, OnInit } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { OpenDMSBackendService, StorageLocationDTO } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UtilitiesService } from '../../../services/utilities.service';

@Component({
  selector: 'app-content-tree',
  standalone: false,
  templateUrl: './content-tree.component.html',
  styleUrl: './content-tree.component.scss'
})
export class ContentTreeComponent implements OnInit {
  storageLocations: StorageLocationDTO[] = [];

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private httpClient: HttpClient, private utilitiesService: UtilitiesService) {
  }

  ngOnInit(): void {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetAllViewableStorageLocationsGet(this.storageService.getAccessToken()).subscribe((storageLocations) => {
      storageLocations.forEach(storageLocation => {
        this.storageLocations.push(storageLocation);
      });
    });
  }
}
