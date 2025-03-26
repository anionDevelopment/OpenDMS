import { Component, EventEmitter, inject, Input, OnInit, Output } from '@angular/core';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';
import { UtilitiesService } from '../../../services/utilities.service';
import saveAs from 'file-saver';
import { Observable } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { EditDocumentDialogComponent } from '../edit-document-dialog/edit-document-dialog.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

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

  @Output()
  documentRemoved: EventEmitter<string/*document-id*/> = new EventEmitter<string>();

  readonly dialog = inject(MatDialog);

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private utilitiesService: UtilitiesService) {
  }

  ngOnInit(): void {
    if (this.documents$) {
      this.documents$.subscribe(newDocumentList => {
        this.documents = newDocumentList;
      });
    }
  }


  removeDocument(document: DocumentPreviewDTO) {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent);
    dialogRef.afterClosed().subscribe(userConfirmedAction => {
      if (userConfirmedAction) {
        this.openDMSBackendService.aPIV1OpenDMSBackendDeleteContainerOrContaineeIdDelete(document.id!, this.storageService.getAccessToken())
          .subscribe(() => {
            this.documents = this.documents.filter(item => item.id != document.id);
            this.documentRemoved.emit(document.id!);
          });
      }
    });
  }

  downloadDocument(document: DocumentPreviewDTO) {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), document.id!)
      .subscribe(document => {
        saveAs(this.utilitiesService.base64toBlob(document.documentContentAsBase64!, "octet/stream"), document.filename!);
      });
  }

  editDocument(document: DocumentPreviewDTO) {
    const dialogRef = this.dialog.open(EditDocumentDialogComponent, {
      data: { document: document },
    });
    dialogRef.afterClosed().subscribe(result => {
      //TODO update something if changed
    });
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
