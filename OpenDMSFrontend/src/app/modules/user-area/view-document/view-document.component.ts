import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { OpenDMSBackendService } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-view-document',
  standalone: false,
  templateUrl: './view-document.component.html',
  styleUrl: './view-document.component.scss'
})
export class ViewDocumentComponent {

  constructor(storageService: StorageService, openDMSBackendService: OpenDMSBackendService) {

    openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGetFromReadableId(storageService.getAccessToken()).subscribe(document => {
      //TODO
    });
  }
}
