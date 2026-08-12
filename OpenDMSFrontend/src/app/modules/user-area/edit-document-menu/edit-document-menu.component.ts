import { Component, EventEmitter, inject, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { DocumentPreviewDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';
import { UtilitiesService } from '../../../services/utilities.service';
import { MatDialog } from '@angular/material/dialog';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';
import { saveAs } from 'file-saver';
import { EditDocumentDialogComponent } from '../edit-document-dialog/edit-document-dialog.component';
import { of, switchMap } from 'rxjs';

@Component({
  selector: 'app-edit-document-menu',
  standalone: false,
  templateUrl: './edit-document-menu.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './edit-document-menu.component.scss'
})
export class EditDocumentMenuComponent {
  @Input()
  documentPreview: DocumentPreviewDTO | null = null;

  @Output()
  documentRemoved: EventEmitter<string/*document-id*/> = new EventEmitter<string>();

  readonly dialog = inject(MatDialog);

  public constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private utilitiesService: UtilitiesService) {
  }

  removeDocument() {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent);
    dialogRef.afterClosed().subscribe(userConfirmedAction => {
      if (userConfirmedAction) {
        this.openDMSBackendService.aPIV3OpenDMSBackendSoftDeleteContainerOrContaineeIdDelete(this.documentPreview!.id!, this.storageService.getAccessToken())
          .subscribe(() => {
            this.documentRemoved.emit(this.documentPreview?.id!);
          });
      }
    });
  }

  downloadDocument() {
    this.openDMSBackendService.aPIV3OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), this.documentPreview!.id!)
      .subscribe(document => {
        saveAs(this.utilitiesService.base64toBlob(document.documentContentAsBase64!, "octet/stream"), document.filename!);
      });
  }

  editDocument() {
    const dialogRef = this.dialog.open(EditDocumentDialogComponent, {
      data: { documentDTO: this.documentPreview },
    });
    dialogRef.afterClosed().pipe(switchMap(result => {
      //the result is undefined when the dialog was closed without using one of its buttons (for example by pressing escape or by clicking the backdrop).
      if (result?.save) {
        const newTitle: string = result.data.documentTitle;
        return this.openDMSBackendService.aPIV3OpenDMSBackendUpdateDocumentTitleDocumentIdPut(this.documentPreview?.id!, this.storageService.getAccessToken(), { value: newTitle }).pipe(switchMap(() => of(newTitle)));
      } else {
        return of(this.documentPreview?.title);
      }
    })).subscribe((newTitle) => {
      this.documentPreview!.title = newTitle;
    });
  }

  viewDocument() {
    this.openDMSBackendService.aPIV3OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), this.documentPreview!.id!)
      .subscribe(documentDTO => {
        var fileURL = window.URL.createObjectURL(this.utilitiesService.base64toBlob(documentDTO.documentContentAsBase64!, documentDTO.mimeType!));
        const tab = window.open()!;
        tab.location.href = fileURL;
      });
  }
}
