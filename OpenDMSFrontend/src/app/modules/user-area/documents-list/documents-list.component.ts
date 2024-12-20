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
        saveAs(this.utilitiesService.base64toBlob(document.documentContentAsBase64!, "octet/stream"), document.filename!);
      });
  }

  editDocument(document: DocumentPreviewDTO) {
    throw new Error('Method not implemented.');
  }

  viewDocument(documentPreviewDTO: DocumentPreviewDTO) {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), documentPreviewDTO.id!)
      .subscribe(documentDTO => {

        var fileURL = window.URL.createObjectURL(this.utilitiesService.base64toBlob(documentDTO.documentContentAsBase64!, documentDTO.mimeType!));
        const tab = window.open()!;
        tab.location.href = fileURL;


        /*
        let fileURL = URL.createObjectURL(this.utilitiesService.base64toBlob(documentDTO.documentContentAsBase64!, documentDTO.mimeType!));
        // create <a> element dynamically
        let fileLink = document.createElement('a');
        fileLink.setAttribute('target', '_blank');
        fileLink.href = fileURL;
        // suggest a name for the downloaded file
        fileLink.download = 'pdf_name';
        // simulate click
        fileLink.click();
        */

        /*
        let fileURL = URL.createObjectURL(this.utilitiesService.base64toBlob(documentDTO.documentContentAsBase64!, documentDTO.mimeType!));
        const iframe = document.createElement('iframe');
        iframe.src = fileURL;
        iframe.width = '100%';
        iframe.height = '100%';
        iframe.style.border = 'none';
        const newWindow = window.open('', '_blank')!;
        newWindow.document.body.appendChild(iframe);
        newWindow.document.title = 'My Custom Title';
        */
      });
  }

  addDocument() {
    let element: HTMLElement = document.querySelector('input[type="file"]') as HTMLElement;
    element.click();
  }

}
