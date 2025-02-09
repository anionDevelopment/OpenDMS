import { Component, EventEmitter, Input, Output } from '@angular/core';
import { DocumentDTO, FolderDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UtilitiesService } from '../../../services/utilities.service';

@Component({
  selector: 'app-add-content-button',
  standalone: false,
  templateUrl: './add-content-button.component.html',
  styleUrl: './add-content-button.component.scss'
})
export class AddContentButtonComponent {
  @Input()
  parentId: string | null | undefined = null;

  @Input()
  userIsAllowedToAddDocument: boolean = true;//TODO set initial value to false

  @Input()
  userIsAllowedToAddFolder: boolean = true;//TODO set initial value to false

  @Output()
  folderAdded: EventEmitter<FolderDTO> = new EventEmitter<FolderDTO>();

  @Output()
  documentAdded: EventEmitter<DocumentDTO> = new EventEmitter<DocumentDTO>();

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {
  }

  addDocument(event: any): void {
    const blob: File = event.target.files[0];
    const reader = new FileReader()
    reader.onload = (e) => {
      const blobAsBaseString = (reader.result! as string).split(',')[1];
      this.openDMSBackendService.aPIV1OpenDMSBackendAddDocumentContainerIdPost(this.parentId!, this.storageService.getAccessToken(), blob.name, blob.name, blobAsBaseString).subscribe((newDocumentId) => {
        this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), newDocumentId).subscribe(newDocument => {
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
    this.openDMSBackendService.aPIV1OpenDMSBackendAddFolderParentFolderIdNamePost(this.parentId!, "New folder", this.storageService.getAccessToken()).subscribe(newFolderId => {
      this.openDMSBackendService.aPIV1OpenDMSBackendGetFolderIdGet(newFolderId, this.storageService.getAccessToken()).subscribe((newFolder) => {
        this.folderAdded.next(newFolder);
      });
    });
  }
}
