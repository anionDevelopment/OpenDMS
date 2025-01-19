import { Component, Input, OnInit } from '@angular/core';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UtilitiesService } from '../../../services/utilities.service';
import saveAs from 'file-saver';
import { Observable, of } from 'rxjs';

@Component({
  selector: 'app-document-table',
  standalone: false,
  templateUrl: './document-table.component.html',
  styleUrl: './document-table.component.scss'
})
export class DocumentTableComponent implements OnInit {

  displayedColumns: string[] = ["id", "name", "importdate", "options"];

  @Input()
  documents$: Observable<DocumentPreviewDTO[]> | null = null;
  documents: DocumentPreviewDTO[] = [];

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private utilitiesService: UtilitiesService) {
  }

  ngOnInit(): void {
    if (this.documents$) {
      this.documents$.subscribe(newDocumentList => {
        this.documents = newDocumentList;
      });
    }
  }

  onDocumentClick(document: DocumentPreviewDTO) {
    this.router.navigate(["user", "document"], { queryParams: { documentId: document.id } });
  }

  removeDocument(document: DocumentPreviewDTO) {
    this.openDMSBackendService.aPIV1OpenDMSBackendDeleteContainerOrContaineeIdDelete(document.id!, this.storageService.getAccessToken())
      .subscribe(() => {
        this.documents = this.documents.filter(item => item.id != document.id);
      });
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
      });
  }


}
