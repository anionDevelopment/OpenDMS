import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { DocumentDTO, FolderDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { MatDialog } from '@angular/material/dialog';
import { EditContainerDialogComponent as EditContainerDialogComponent } from '../edit-container-dialog/edit-container-dialog.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-edit-container-menu',
  standalone: false,
  templateUrl: './edit-container-menu.component.html',
  styleUrl: './edit-container-menu.component.scss'
})
export class EditContainerMenuComponent {
  @Input()
  containerId: string | null | undefined = null;

  @Input()
  userIsAllowedToAddDocument: boolean = true;//TODO set initial value to false and set only to true when user has permission to do that

  @Input()
  userIsAllowedToAddFolder: boolean = true;//TODO set initial value to false and set only to true when user has permission to do that

  @Output()
  folderAdded: EventEmitter<FolderDTO> = new EventEmitter<FolderDTO>();

  @Output()
  documentAdded: EventEmitter<DocumentDTO> = new EventEmitter<DocumentDTO>();

  @Output()
  containerRemoved: EventEmitter<void> = new EventEmitter<void>();

  readonly dialog = inject(MatDialog);
  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  addDocument(event: any): void {
    const blob: File = event.target.files[0];
    const reader = new FileReader()
    reader.onload = (e) => {
      const blobAsBaseString = (reader.result! as string).split(',')[1];
      this.openDMSBackendService.aPIV2OpenDMSBackendAddDocumentContainerIdPost(this.containerId!, this.storageService.getAccessToken(), blob.name, blob.name, [], '"' + blobAsBaseString + '"').subscribe((newDocumentId) => {
        this.openDMSBackendService.aPIV2OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), newDocumentId).subscribe(newDocument => {
          this.documentAdded.next(newDocument);
        });
      });
    }
    reader.onerror = (error) => {
      console.error(error);
    }
    reader.readAsDataURL(blob);
  }

  addFolder(): void {
    this.openDMSBackendService.aPIV2OpenDMSBackendAddFolderParentFolderIdNamePost(this.containerId!, "New folder", this.storageService.getAccessToken()).subscribe(newFolderId => {
      this.openDMSBackendService.aPIV2OpenDMSBackendGetFolderIdGet(newFolderId, this.storageService.getAccessToken()).subscribe((newFolder) => {
        this.folderAdded.next(newFolder);
      });
    });
  }

  edit(): void {
    const dialogRef = this.dialog.open(EditContainerDialogComponent, {
      data: { document: document },
    });
    dialogRef.afterClosed().subscribe(result => {
      //TODO update something if changed
    });
  }
  removeContainer(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent);
    dialogRef.afterClosed().subscribe(userConfirmedAction => {
      if (userConfirmedAction) {
        this.openDMSBackendService.aPIV2OpenDMSBackendSoftDeleteContainerOrContaineeIdDelete(this.containerId!, this.storageService.getAccessToken())
          .subscribe(() => {
            this.containerRemoved.emit();
          });
      }
    });
  }
}
