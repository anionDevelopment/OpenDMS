import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { DocumentPreviewDTO } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-link-to-document',
  standalone: false,
  templateUrl: './link-to-document.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './link-to-document.component.scss'
})
export class LinkToDocumentComponent {
  @Input()
  documentPreview: DocumentPreviewDTO | null = null;

}
