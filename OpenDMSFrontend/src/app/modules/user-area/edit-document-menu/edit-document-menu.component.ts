import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';
import { UtilitiesService } from '../../../services/utilities.service';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import saveAs from 'file-saver';
import { EditDocumentDialogComponent } from '../edit-document-dialog/edit-document-dialog.component';

@Component({
  selector: 'app-edit-document-menu',
  standalone: false,
  templateUrl: './edit-document-menu.component.html',
  styleUrl: './edit-document-menu.component.scss'
})
export class EditDocumentMenuComponent {
  @Input()
  document: DocumentPreviewDTO | null = null;

  @Output()
  documentRemoved: EventEmitter<string/*document-id*/> = new EventEmitter<string>();

  readonly dialog = inject(MatDialog);

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private utilitiesService: UtilitiesService) {
  }

  removeDocument() {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent);
    dialogRef.afterClosed().subscribe(userConfirmedAction => {
      if (userConfirmedAction) {
        this.openDMSBackendService.aPIV1OpenDMSBackendDeleteContainerOrContaineeIdDelete(this.document!.id!, this.storageService.getAccessToken())
          .subscribe(() => {
            this.documentRemoved.emit(this.document?.id!);
          });
      }
    });
  }

  downloadDocument() {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), this.document!.id!)
      .subscribe(document => {
        saveAs(this.utilitiesService.base64toBlob(document.documentContentAsBase64!, "octet/stream"), document.filename!);
      });
  }

  editDocument() {
    const dialogRef = this.dialog.open(EditDocumentDialogComponent, {
      data: { document: document },
    });
    dialogRef.afterClosed().subscribe(result => {
      //TODO trigger event
    });
  }

  viewDocument() {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), this.document!.id!)
      .subscribe(documentDTO => {
        var fileURL = window.URL.createObjectURL(this.utilitiesService.base64toBlob(documentDTO.documentContentAsBase64!, documentDTO.mimeType!));
        const tab = window.open()!;
        tab.location.href = fileURL;
      });
  }
}
