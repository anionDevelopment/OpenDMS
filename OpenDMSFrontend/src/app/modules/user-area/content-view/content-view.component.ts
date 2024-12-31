import { Component, Input, OnInit } from '@angular/core';
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
export class ContentViewComponent implements OnInit {
  @Input()
  title: string | null | undefined = null;

  @Input()
  parentContainerId: string | null | undefined = null;

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

  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, private router: Router, private httpClient: HttpClient, private utilitiesService: UtilitiesService) {

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
    this.openDMSBackendService.aPIV1OpenDMSBackendGetDocumentPreviewGet(this.storageService.getAccessToken(), documentId).subscribe(document => this.addDocument(document));
  }

  loadFolder(folderId: string): void {
    this.openDMSBackendService.aPIV1OpenDMSBackendGetFolderIdGet(folderId, this.storageService.getAccessToken()).subscribe(folder => this.addFolder(folder));
  }

  onFolderAdded(newFolder: FolderDTO): void {
    this.addFolder(newFolder);
  }

  onDocumentAdded(newDocument: DocumentDTO): void {
    this.addDocument(newDocument);
  }

  addDocument(document: DocumentDTO) {
    this.documents.push(document);
    this.documents = this.documents.sort((a, b) => a.title!.localeCompare(b.title!));
    this.documents$.next([...this.documents]);
  }
  addFolder(folder: FolderDTO) {
    this.folders.push(folder);
    this.folders = this.folders.sort((a, b) => a.name!.localeCompare(b.name!));
    this.folders$.next([...this.folders]);
  }
}
