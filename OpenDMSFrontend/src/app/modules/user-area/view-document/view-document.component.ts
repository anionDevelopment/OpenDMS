import { Component } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentDTO, OpenDMSBackendService, TagDTO } from '../../../generated/open-dms-backend';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-view-document',
  standalone: false,
  templateUrl: './view-document.component.html',
  styleUrl: './view-document.component.scss'
})
export class ViewDocumentComponent {
  document: DocumentDTO | null = null;
  constructor(storageService: StorageService, openDMSBackendService: OpenDMSBackendService, route: ActivatedRoute) {
    route.params.subscribe(params => {
      const readableId = params['readableId'];
      openDMSBackendService.aPIV1OpenDMSBackendGetDocumentFromReadableIdGet(storageService.getAccessToken(), readableId).subscribe(document => {
        this.document = document;
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
}
