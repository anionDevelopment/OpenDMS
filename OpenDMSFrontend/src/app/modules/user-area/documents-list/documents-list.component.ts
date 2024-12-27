import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentPreviewDTO, FolderDTO, OpenDMSBackendService, StorageLocationDTO } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import saveAs from 'file-saver';
import { UtilitiesService } from '../../../services/utilities.service';
import { FactoryTarget } from '@angular/compiler';

@Component({
  selector: 'app-documents-list',
  standalone: false,
  templateUrl: './documents-list.component.html',
  styleUrl: './documents-list.component.scss'
})
export class DocumentsListComponent {
  displayedColumns: string[] = ["id", "name", "importdate", "options"];
  allAvailableStorageLocations: StorageLocationDTO[] = [];
  allAvailableFolder: FolderDTO[] = [];
  allAvailableDocumentPreviews: DocumentPreviewDTO[] = [];
  private entireContentLoaded: boolean = false;
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
      });
  }

  addDocument(event: any) {
    const blob: File = event.target.files[0];
    const reader = new FileReader()
    reader.onload = (e) => {
      const blobAsBaseString = (reader.result! as string).split(',')[1];
      this.openDMSBackendService.aPIV1OpenDMSBackendAddDocumentContainerIdPost("storageLocationOrFolderId", this.storageService.getAccessToken(), blob.name, blob.name, blobAsBaseString).subscribe((documentId) => {
        this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), documentId).subscribe(document => {
          this.loaddocument(document);
        });
      });
    }
    reader.onerror = () => {
      console.log('error');
    }
    reader.readAsDataURL(blob);
  }

  allDocumentsClicked() {
    console.log("clicked");
    if (!this.entireContentLoaded) {
      this.entireContentLoaded = true;
      this.openDMSBackendService.aPIV1OpenDMSBackendGetAllViewableStorageLocationsGet(this.storageService.getAccessToken()).subscribe(storageLocations => {
        this.allAvailableStorageLocations = storageLocations;
        this.allAvailableStorageLocations.forEach(storageLocation => {
          this.loadChildrenOfStorageLocation(storageLocation);
        });
      });
    }
  }

  loadChildrenOfStorageLocation(storageLocation: StorageLocationDTO) {
    storageLocation.containedFolderIds?.forEach(folderId => {
      this.openDMSBackendService.aPIV1OpenDMSBackendGetFolderIdGet(folderId, this.storageService.getAccessToken()).subscribe((folder) => {
        this.allAvailableFolder.push(folder);
        this.loadChildrenOfFolder(folder);
      });
    });
    storageLocation.containedDocumentIds?.forEach(documentId => {
      this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(documentId, this.storageService.getAccessToken()).subscribe(document => {
        this.loaddocument(document);
      });
    });
  }

  loadChildrenOfFolder(folder: FolderDTO) {
    folder.containedFolderIds?.forEach(folderId => {
      this.openDMSBackendService.aPIV1OpenDMSBackendGetFolderIdGet(folderId, this.storageService.getAccessToken()).subscribe((folder) => {
        this.allAvailableFolder.push(folder);
        this.loadChildrenOfFolder(folder);
      });
    });
    folder.containedDocumentIds?.forEach(documentId => {
      this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentGet(documentId, this.storageService.getAccessToken()).subscribe(document => {
        this.loaddocument(document);
      });
    });
  }

  loaddocument(document: DocumentPreviewDTO) {
    this.allAvailableDocumentPreviews.push(document);
  }
}
