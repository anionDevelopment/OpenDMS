import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentDTO, DocumentPreviewDTO, OpenDMSBackendService, TagDTO } from '../../../generated/open-dms-backend';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-view-document',
  standalone: false,
  templateUrl: './view-document.component.html',
  styleUrl: './view-document.component.scss'
})
export class ViewDocumentComponent {
  document: DocumentDTO | null = null;
  documentPreview: DocumentPreviewDTO | null = null;
  constructor(storageService: StorageService, openDMSBackendService: OpenDMSBackendService, route: ActivatedRoute, private router: Router) {
    route.params.subscribe(params => {
      const readableId = params['readableId'];
      openDMSBackendService.aPIV2OpenDMSBackendGetDocumentFromReadableIdGet(storageService.getAccessToken(), readableId).subscribe(document => {
        this.document = document;
        openDMSBackendService.aPIV2OpenDMSBackendGetDocumentPreviewGet(storageService.getAccessToken(), document.id!).subscribe(documentPreview => {
          this.documentPreview = documentPreview;
        });
      });
    });
  }

  formatTags(tags: Set<TagDTO>): string {
    let set: Set<TagDTO> = tags;
    if (!set) {
      set = new Set<TagDTO>();
    }
    return Array.from(set).map(tag => tag.name).join(", ");//TODO refactor to make this list ediable in the ui and consider color of tags
  }

  onDocumentRemoved(): void {
    this.router.navigate(['user', 'dashboard']);
  }
}
