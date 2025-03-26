import { Component, Input } from '@angular/core';
import { DocumentPreviewDTO } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-link-to-document',
  standalone: false,
  templateUrl: './link-to-document.component.html',
  styleUrl: './link-to-document.component.scss'
})
export class LinkToDocumentComponent {
  @Input()
  documentPreview: DocumentPreviewDTO | null = null;

}
