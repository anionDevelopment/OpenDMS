import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentDTO, DocumentPreviewDTO, FolderDTO, OpenDMSBackendService } from '../../../generated/open-dms-backend';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { UtilitiesService } from '../../../services/utilities.service';
import { BehaviorSubject, Subject } from 'rxjs';

@Component({
  selector: 'app-content-view',
  standalone: false,
  templateUrl: './content-view.component.html',
  styleUrl: './content-view.component.scss'
})
export class ContentViewComponent implements OnInit {//shows the content of a container

  @Input()
  title: string | null | undefined = null;

  @Input()
  containerId: string | null | undefined = null;

  @Input()
  userIsAllowedToAddDocuments: boolean = false;

  @Input()
  isFolder: boolean = false;

  @Input()
  documentIds: string[] | null | undefined = []

  @Input()
  folderIds: string[] | null | undefined = []

  documents: DocumentPreviewDTO[] = [];
  documents$: Subject<DocumentPreviewDTO[]> = new BehaviorSubject<DocumentPreviewDTO[]>([]);
  folders: FolderDTO[] = []
  folders$: Subject<FolderDTO[]> = new BehaviorSubject<FolderDTO[]>([]);

  @Output()
  containerRemoved: EventEmitter<string/*container-id*/> = new EventEmitter<string>();

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService) {

  }

  ngOnInit(): void {
    if (this.documentIds) {
      this.documentIds.forEach(documentId => this.loadDocument(documentId));
    }
    if (this.folderIds) {
      this.folderIds.forEach(folderId => this.loadFolder(folderId));
    }
  }

  loadDocument(documentId: string): void {
    this.openDMSBackendService.aPIV2OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), documentId).subscribe(document => this.addDocument(document));
  }

  loadFolder(folderId: string): void {
    this.openDMSBackendService.aPIV2OpenDMSBackendGetFolderIdGet(folderId, this.storageService.getAccessToken()).subscribe(folder => this.addFolder(folder));
  }

  onFolderAdded(newFolder: FolderDTO): void {
    this.addFolder(newFolder);
  }

  onDocumentAdded(newDocument: DocumentDTO): void {
    this.addDocument(newDocument);
  }

  private addDocument(document: DocumentDTO) {
    this.documents.push(document);
    this.documents = this.documents.sort((a, b) => a.title!.localeCompare(b.title!));
    this.documents$.next([...this.documents]);
  }
  private removeDocument(documentId: string) {
    this.documents = this.documents.filter(document => document.id! != documentId);
    this.documents$.next([...this.documents]);
  }
  private removeFolder(folderId: string) {
    this.folders = this.folders.filter(folder => folder.id! != folderId);
    this.folders$.next([...this.folders]);
  }
  addFolder(folder: FolderDTO) {
    this.folders.push(folder);
    this.folders = this.folders.sort((a, b) => a.name!.localeCompare(b.name!));
    this.folders$.next([...this.folders]);
  }
  onDocumentRemoved(documentId: string): void {
    this.removeDocument(documentId);
  }
  onContainerRemoved(containerId: string) {
    this.removeFolder(containerId);
  }
  onRemoved() {
    this.containerRemoved.emit(this.containerId!);
  }
} 
