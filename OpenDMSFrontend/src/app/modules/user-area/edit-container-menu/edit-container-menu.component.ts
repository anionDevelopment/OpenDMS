import { Component, EventEmitter, inject, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { DocumentDTO, FolderDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { MatDialog } from '@angular/material/dialog';
import { EditContainerDialogComponent } from '../edit-container-dialog/edit-container-dialog.component';
import { ConfirmationDialogComponent } from '../confirmation-dialog/confirmation-dialog.component';

@Component({
  selector: 'app-edit-container-menu',
  standalone: false,
  templateUrl: './edit-container-menu.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './edit-container-menu.component.scss'
})
export class EditContainerMenuComponent {
  @Input()
  containerId: string | null | undefined = null;

  @Input()
  containerTitle: string | null | undefined = null;

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

  @Output()
  containerTitleChanged: EventEmitter<string> = new EventEmitter<string>();

  readonly dialog = inject(MatDialog);
  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  addDocument(event: any): void {
    const blob: File = event.target.files[0];
    const reader = new FileReader()
    reader.onload = (e) => {
      const blobAsBaseString = (reader.result! as string).split(',')[1];
      this.openDMSBackendService.aPIV3OpenDMSBackendAddDocumentContainerIdPost(this.containerId!, this.storageService.getAccessToken(), blob.name, blob.name, [], '"' + blobAsBaseString + '"').subscribe((newDocumentId) => {
        this.openDMSBackendService.aPIV3OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), newDocumentId).subscribe(newDocument => {
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
    this.openDMSBackendService.aPIV3OpenDMSBackendAddFolderParentFolderIdNamePost(this.containerId!, "New folder", this.storageService.getAccessToken()).subscribe(newFolderId => {
      this.openDMSBackendService.aPIV3OpenDMSBackendGetFolderIdGet(newFolderId, this.storageService.getAccessToken()).subscribe((newFolder) => {
        this.folderAdded.next(newFolder);
      });
    });
  }

  edit(): void {
    const dialogRef = this.dialog.open(EditContainerDialogComponent, {
      data: { containerTitle: this.containerTitle ?? '' },
    });
    dialogRef.afterClosed().subscribe(result => {
      if (result?.save) {
        const newTitle: string = result.data.containerTitle;
        this.openDMSBackendService.aPIV3OpenDMSBackendRenameContainerIdNewNamePost(this.containerId!, newTitle, this.storageService.getAccessToken()).subscribe(() => {
          this.containerTitleChanged.emit(newTitle);
        });
      }
    });
  }

  removeContainer(): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent);
    dialogRef.afterClosed().subscribe(userConfirmedAction => {
      if (userConfirmedAction) {
        this.openDMSBackendService.aPIV3OpenDMSBackendSoftDeleteContainerOrContaineeIdDelete(this.containerId!, this.storageService.getAccessToken())
          .subscribe(() => {
            this.containerRemoved.emit();
          });
      }
    });
  }
}
