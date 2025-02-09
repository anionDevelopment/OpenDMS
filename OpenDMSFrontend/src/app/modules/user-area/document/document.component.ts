import { Component } from '@angular/core';
import { DocumentDTO, DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { ActivatedRoute } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';

@Component({
  selector: 'app-document',
  standalone: false,
  templateUrl: './document.component.html',
  styleUrl: './document.component.scss'
})
export class DocumentComponent {
  currentDocument$: Observable<DocumentDTO | null>;
  constructor(route: ActivatedRoute, openDMSBackendService: OpenDMSBackendService, storgeService: StorageService) {
    this.currentDocument$ = route.queryParams.pipe(switchMap(params => {
      if (params["documentId"]) {
        return openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(params["documentId"], storgeService.getAccessToken());
      } else {
        return of(null);
      }
    }));
  }
}
