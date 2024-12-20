import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';

@Component({
  selector: 'app-documents-list',
  standalone: false,
  templateUrl: './documents-list.component.html',
  styleUrl: './documents-list.component.scss'
})
export class DocumentsListComponent {
  displayedColumns: string[] = ["id", "name", "importdate", "options"];
  allAvailableDocumentPreviews: DocumentPreviewDTO[] = [];

  constructor(storageService: StorageService, openDMSBackendService: OpenDMSBackendService, private router: Router) {
    openDMSBackendService.aPIV1OpenDMSBackendGetLatestDocumentsGet(storageService.getAccessToken()).subscribe(documents => {
      this.allAvailableDocumentPreviews = documents;
    });
  }

  onDocumentClick(document: DocumentPreviewDTO) {
    this.router.navigate(["user", "document"], { queryParams: { documentId: document.id } });
  }

  removeDocument(document: DocumentPreviewDTO) {
    throw new Error('Method not implemented.');
  }

  downloadDocument(document: DocumentPreviewDTO) {
    throw new Error('Method not implemented.');
  }

  editDocument(document: DocumentPreviewDTO) {
    throw new Error('Method not implemented.');
  }

  viewDocument(document: DocumentPreviewDTO) {
    throw new Error('Method not implemented.');
  }

  addDocument() {
    throw new Error('Method not implemented.');
  }

}
