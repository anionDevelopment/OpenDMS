import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import saveAs from 'file-saver';
import { UtilitiesService } from '../../../services/utilities.service';

@Component({
  selector: 'app-documents-list',
  standalone: false,
  templateUrl: './documents-list.component.html',
  styleUrl: './documents-list.component.scss'
})
export class DocumentsListComponent {
  displayedColumns: string[] = ["id", "name", "importdate", "options"];
  allAvailableDocumentPreviews: DocumentPreviewDTO[] = [];

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private httpClient: HttpClient, private utilitiesService: UtilitiesService) {
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
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), document.id!)
      .subscribe(document => {
        console.log(document);
        const x = window.atob(document.documentContent as string);
        console.log(x);
        saveAs(new Blob([x], {
          type: 'application/pdf'
        }), document.filename! as string);
      });
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
