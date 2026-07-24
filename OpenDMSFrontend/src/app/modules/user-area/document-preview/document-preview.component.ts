import { Component, Input, OnInit } from '@angular/core';
import { DocumentDTO, DocumentPreviewDTO } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-document-preview',
  standalone: false,
  templateUrl: './document-preview.component.html',
  styleUrl: './document-preview.component.scss'
})
export class DocumentPreviewComponent implements OnInit {

  @Input()
  documentPreview: DocumentPreviewDTO | null = null;

  image: string | null = null;

  constructor() {
  }

  ngOnInit(): void {
    if (this.documentPreview) {
      this.image = 'data:image/png;base64,' + this.documentPreview?.previewAsBase64;
    }
  }

  popupVisible = false;
  popupX = 0;
  popupY = 0;

  updatePopupPosition(event: MouseEvent) {
    this.popupVisible = true;
    const offset = 10;
    this.popupX = event.clientX + offset;
    this.popupY = event.clientY + offset;
    const maxX = window.innerWidth - 420;
    const maxY = window.innerHeight - 420;
    if (this.popupX > maxX) this.popupX = maxX;
    if (this.popupY > maxY) this.popupY = maxY;
  }
}
