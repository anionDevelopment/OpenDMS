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
  doc: DocumentDTO | null = null;
  documentPreview: DocumentPreviewDTO | null = null;
  aiSummaryIsGenerating: boolean = false;
  constructor(private storageService: StorageService, private openDMSBackendService: OpenDMSBackendService, route: ActivatedRoute, private router: Router) {
    route.params.subscribe(params => {
      const readableId = params['readableId'];
      openDMSBackendService.aPIV3OpenDMSBackendGetDocumentFromReadableIdGet(storageService.getAccessToken(), readableId).subscribe(document => {
        this.doc = document;
        openDMSBackendService.aPIV3OpenDMSBackendGetDocumentPreviewGet(storageService.getAccessToken(), document.id!).subscribe(documentPreview => {
          this.documentPreview = documentPreview;
        });
      });
    });
  }

  aiSummaryIsAvailable(): boolean {
    return !!(this.doc && (this.doc.aiSummaryShort || this.doc.aiSummaryLong));
  }

  generateAISummary(): void {
    if (!this.doc || this.aiSummaryIsGenerating) {
      return;
    }
    this.aiSummaryIsGenerating = true;
    this.openDMSBackendService.aPIV3OpenDMSBackendGenerateAISummaryDocumentIdPost(this.doc.id!, this.storageService.getAccessToken()).subscribe({
      next: updatedDocument => {
        this.doc = updatedDocument;
        this.aiSummaryIsGenerating = false;
      },
      error: () => {
        this.aiSummaryIsGenerating = false;
      }
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
