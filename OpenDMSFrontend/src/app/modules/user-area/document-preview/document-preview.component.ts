import { Component, Input } from '@angular/core';
import { DocumentDTO } from '../../../generated/open-dms-backend';

@Component({
  selector: 'app-document-preview',
  standalone: false,
  templateUrl: './document-preview.component.html',
  styleUrl: './document-preview.component.scss'
})
export class DocumentPreviewComponent {
  @Input()
  document: DocumentDTO | null = null;

}
