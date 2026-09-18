import { Component, ChangeDetectionStrategy } from '@angular/core';
import { StorageService } from '../../../services/storage.service';
import { DocumentDTO, DocumentPreviewDTO, OpenDMSBackendService, TagDTO } from '../../../generated/open-dms-backend';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-view-document',
  standalone: false,
  templateUrl: './view-document.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
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

  /**
   * The tags which are assigned to the shown document. The generated type declares them as a set, but the backend
   * delivers them as a json-array, so they are converted into the list which the user-interface works with.
   */
  get assignedTags(): TagDTO[] {
    return this.doc?.tags ? Array.from(this.doc.tags) : [];
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

  /**
   * Loads the shown document again. The components which change the tags or the metadata of the document report
   * their change instead of updating the document themselves, so that this component stays the single place which
   * holds the state of the document.
   */
  reloadDocument(): void {
    if (!this.doc?.id) {
      return;
    }
    this.openDMSBackendService.aPIV3OpenDMSBackendGetDocumentGet(this.storageService.getAccessToken(), this.doc.id).subscribe(document => this.doc = document);
  }

  onDocumentRemoved(): void {
    this.router.navigate(['user', 'dashboard']);
  }
}
