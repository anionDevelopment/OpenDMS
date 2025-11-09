import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { BehaviorSubject } from 'rxjs';

@Component({
  selector: 'app-documents-list',
  standalone: false,
  templateUrl: './documents-list.component.html',
  styleUrl: './documents-list.component.scss'
})
export class DocumentsListComponent {

  latestDocuments$: BehaviorSubject<DocumentPreviewDTO[]> = new BehaviorSubject<DocumentPreviewDTO[]>([]);

  constructor(storageService: StorageService, openDMSBackendService: OpenDMSBackendService) {
    openDMSBackendService.aPIV2OpenDMSBackendGetLatestDocumentsGet(storageService.getAccessToken()).subscribe(documents => {
      var sortedDocuments = documents.sort((a, b) => {
        const aD: Date = this.getNewestDate(a);
        const bD: Date = this.getNewestDate(b);
        if (aD < bD) {
          return 1;
        }
        if (aD > bD) {
          return -1;
        }
        return 0;
      });
      this.latestDocuments$.next(sortedDocuments);
    });
  }

  private getNewestDate(a: DocumentPreviewDTO): Date {
    if (a.lastEditDate !== null && a.lastEditDate !== undefined) {
      return this.parseDate(a.lastEditDate!);
    } else {
      return this.parseDate(a.importDate!);
    }
  }

  private parseDate(str: string): Date {
    const result: Date = new Date(str);
    return result;
  }
  private removeDocument(documentId: string) {
    var latestDocuments = this.latestDocuments$.value.filter(document => document.id! != documentId);
    this.latestDocuments$.next([...latestDocuments]);
  }
  onDocumentRemoved(documentId: string): void {
    this.removeDocument(documentId);
  }
}
